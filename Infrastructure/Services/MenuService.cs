using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Infrastructure.Persistence;
using RestaurantMenuPlatform.Domain.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class MenuService : IMenuService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IEntitlementService? _entitlementService;
    private readonly IAuditLogService? _auditLogService;
    private readonly ILookupService? _lookupService;
    private readonly ICurrentUserContext? _currentUser;

    public MenuService(
        AppDbContext db,
        ITenantContext tenantContext,
        IEntitlementService? entitlementService = null,
        IAuditLogService? auditLogService = null,
        ILookupService? lookupService = null,
        ICurrentUserContext? currentUser = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _entitlementService = entitlementService;
        _auditLogService = auditLogService;
        _lookupService = lookupService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MenuListDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _db.Menus.AsNoTracking();
        if (_currentUser?.BranchId is Guid branchId)
            query = query.Where(x => x.BranchMenus.Any(b => b.BranchId == branchId && b.IsActive));

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new MenuListDto(
                x.Id,
                x.Name,
                x.Slug,
                x.IsGlobal,
                x.Status,
                x.Categories.Count,
                x.Categories.SelectMany(c => c.Items).Count(),
                x.NameAr,
                x.Description,
                x.DescriptionAr,
                x.MenuTypeCode,
                x.ScopeCode,
                x.SortOrder,
                x.BranchMenus.Count(b => b.IsActive)))
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _db.Menus
            .Where(x => _currentUser == null || !_currentUser.BranchId.HasValue ||
                x.BranchMenus.Any(b => b.BranchId == _currentUser.BranchId.Value && b.IsActive));
        var menu = await query
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Categories)
                .ThenInclude(x => x.Items)
                    .ThenInclude(x => x.Ingredients)
                        .ThenInclude(x => x.Ingredient)
            .Include(x => x.Categories)
                .ThenInclude(x => x.Items)
                    .ThenInclude(x => x.Allergens)
                        .ThenInclude(x => x.Allergen)
            .Include(x => x.Categories)
                .ThenInclude(x => x.Items)
                    .ThenInclude(x => x.Images)
            .Include(x => x.Categories)
                .ThenInclude(x => x.Items)
                    .ThenInclude(x => x.Modifiers)
            .Include(x => x.BranchMenus)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (menu is null)
            return null;

        var categories = menu.Categories
            .OrderBy(x => x.SortOrder)
            .Select(category => new MenuCategoryDto(
                category.Id,
                category.Name,
                category.SortOrder,
                category.Items
                    .OrderBy(x => x.SortOrder)
                    .Select(ToItemDto)
                    .ToList(),
                category.NameAr,
                category.Description,
                category.DescriptionAr,
                category.ClassificationCode,
                category.ParentCategoryId,
                category.MenuId))
            .ToList();
        var branchIds = _currentUser?.BranchId is Guid scopedBranchId
            ? menu.BranchMenus
                .Where(x => x.BranchId == scopedBranchId && x.IsActive)
                .Select(x => x.BranchId)
                .Distinct()
                .ToList()
            : menu.BranchMenus.Select(x => x.BranchId).Distinct().ToList();
        var assignedBranches = await _db.Branches.AsNoTracking()
            .Where(x => branchIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .Select(x => new MenuBranchDto(x.Id, x.Name, x.Slug, x.IsActive))
            .ToListAsync(cancellationToken);
        return new MenuDetailsDto(menu.Id, menu.Name, menu.Slug, menu.IsGlobal, menu.Status, categories, menu.NameAr, menu.MenuTypeCode, menu.ScopeCode, branchIds, menu.Description, menu.DescriptionAr, menu.BrandPrimaryColor, menu.BrandAccentColor, menu.SortOrder, assignedBranches);
    }

    public async Task<MenuListDto> CreateAsync(MenuInput input, CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        EnsureName(input.Name, "Menu name");
        await EnsureBrandingEntitlementAsync(input, cancellationToken);
        Menu? menu = null;
        await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        menu = new Menu
        {
            TenantId = RequireTenant(),
            Name = input.Name.Trim(),
            NameEn = input.Name.Trim(),
            NameAr = NullIfEmpty(input.NameAr),
            Description = NullIfEmpty(input.Description),
            DescriptionAr = NullIfEmpty(input.DescriptionAr),
            Slug = await CreateUniqueSlugAsync(input.Name, cancellationToken),
            IsGlobal = ResolveScope(input.ScopeCode, input.IsGlobal),
            MenuTypeCode = await EnsureLookupAsync(LookupTypes.MenuType, input.MenuTypeCode, cancellationToken),
            ScopeCode = await EnsureLookupAsync(LookupTypes.MenuScope, input.ScopeCode, cancellationToken),
            BrandPrimaryColor = NormalizeColor(input.BrandPrimaryColor, "primary menu color"),
            BrandAccentColor = NormalizeColor(input.BrandAccentColor, "accent menu color"),
            SortOrder = input.SortOrder
        };
        _db.Menus.Add(menu);
        await _db.SaveChangesAsync(cancellationToken);
        await ReplaceBranchAssignmentsAsync(menu, input.BranchIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        });
        await WriteAuditAsync("menu.created", menu!.Id, null, new { menu.Name, menu.Slug, menu.IsGlobal, menu.Status }, cancellationToken);
        return await ToListDtoAsync(menu.Id, cancellationToken);
    }

    public async Task<MenuListDto?> UpdateAsync(Guid id, MenuInput input, CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        EnsureName(input.Name, "Menu name");
        var hasBrandingFeature = _entitlementService is null ||
            await _entitlementService.HasFeatureAsync(FeatureKeys.CustomBranding, cancellationToken);
        await EnsureBrandingEntitlementAsync(input, cancellationToken);
        var menu = await _db.Menus.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (menu is null)
            return null;
        var oldValue = new { menu.Name, menu.IsGlobal, menu.Status, menu.ScopeCode, menu.MenuTypeCode, menu.SortOrder };
        menu.Name = input.Name.Trim();
        menu.NameEn = input.Name.Trim();
        menu.NameAr = NullIfEmpty(input.NameAr);
        menu.Description = NullIfEmpty(input.Description);
        menu.DescriptionAr = NullIfEmpty(input.DescriptionAr);
        menu.IsGlobal = ResolveScope(input.ScopeCode, input.IsGlobal);
        menu.MenuTypeCode = await EnsureLookupAsync(LookupTypes.MenuType, input.MenuTypeCode, cancellationToken);
        menu.ScopeCode = await EnsureLookupAsync(LookupTypes.MenuScope, input.ScopeCode, cancellationToken);
        // A downgraded tenant cannot submit the disabled palette controls. Keep
        // any existing branding in that case instead of clearing it silently;
        // an explicit color POST is still rejected by the entitlement guard.
        if (hasBrandingFeature ||
            !string.IsNullOrWhiteSpace(input.BrandPrimaryColor) ||
            !string.IsNullOrWhiteSpace(input.BrandAccentColor))
        {
            menu.BrandPrimaryColor = NormalizeColor(input.BrandPrimaryColor, "primary menu color");
            menu.BrandAccentColor = NormalizeColor(input.BrandAccentColor, "accent menu color");
        }
        menu.SortOrder = input.SortOrder;
        menu.UpdatedAtUtc = DateTime.UtcNow;
        await ReplaceBranchAssignmentsAsync(menu, input.BranchIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("menu.updated", menu.Id, oldValue, new { menu.Name, menu.IsGlobal, menu.Status }, cancellationToken);
        return await ToListDtoAsync(menu.Id, cancellationToken);
    }

    public async Task<MenuListDto?> SetStatusAsync(
        Guid id,
        RestaurantMenuPlatform.Domain.Enums.MenuStatus status,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        var menu = await _db.Menus.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (menu is null)
            return null;

        if (status == RestaurantMenuPlatform.Domain.Enums.MenuStatus.Published)
        {
            if (!await _db.BranchMenus.AnyAsync(x => x.MenuId == id && x.IsActive, cancellationToken))
                throw new ArgumentException("A menu must be assigned to an active branch before it can be published.");
            if (!await _db.MenuItems.AnyAsync(x => x.MenuCategory.MenuId == id && x.IsAvailable, cancellationToken))
                throw new ArgumentException("A menu must contain at least one available product before it can be published.");
        }

        var oldValue = new { menu.Status };
        menu.Status = status;
        menu.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("menu.status-changed", menu.Id, oldValue, new { menu.Status }, cancellationToken);
        return await ToListDtoAsync(menu.Id, cancellationToken);
    }

    public async Task<MenuCategoryDto?> CreateCategoryAsync(
        Guid menuId,
        MenuCategoryInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        EnsureName(input.Name, "Category name");
        if (!await _db.Menus.AnyAsync(x => x.Id == menuId, cancellationToken))
            return null;

        var category = new MenuCategory
        {
            TenantId = RequireTenant(),
            MenuId = menuId,
            Name = input.Name.Trim(),
            NameEn = input.Name.Trim(),
            NameAr = NullIfEmpty(input.NameAr),
            Description = NullIfEmpty(input.Description),
            DescriptionAr = NullIfEmpty(input.DescriptionAr),
            ClassificationCode = await EnsureLookupAsync(LookupTypes.CategoryType, input.ClassificationCode, cancellationToken),
            ParentCategoryId = await ValidateParentAsync(menuId, input.ParentCategoryId, null, cancellationToken),
            SortOrder = input.SortOrder,
            IsActive = true
        };
        _db.MenuCategories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("menu-category.created", category.Id, null, new { category.MenuId, category.Name, category.SortOrder }, cancellationToken);
        return new MenuCategoryDto(category.Id, category.Name, category.SortOrder, [], category.NameAr, category.Description, category.DescriptionAr, category.ClassificationCode, category.ParentCategoryId, category.MenuId);
    }

    public async Task<MenuCategoryDto?> UpdateCategoryAsync(Guid categoryId, MenuCategoryInput input, CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        EnsureName(input.Name, "Category name");
        var category = await _db.MenuCategories.SingleOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
        if (category is null) return null;
        category.Name = input.Name.Trim();
        category.NameEn = input.Name.Trim();
        category.NameAr = NullIfEmpty(input.NameAr);
        category.Description = NullIfEmpty(input.Description);
        category.DescriptionAr = NullIfEmpty(input.DescriptionAr);
        category.ClassificationCode = await EnsureLookupAsync(LookupTypes.CategoryType, input.ClassificationCode, cancellationToken);
        category.ParentCategoryId = await ValidateParentAsync(category.MenuId, input.ParentCategoryId, category.Id, cancellationToken);
        category.SortOrder = input.SortOrder;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("menu-category.updated", category.Id, null, new { category.MenuId, category.Name, category.SortOrder, category.ParentCategoryId }, cancellationToken);
        return new MenuCategoryDto(category.Id, category.Name, category.SortOrder, [], category.NameAr, category.Description, category.DescriptionAr, category.ClassificationCode, category.ParentCategoryId, category.MenuId);
    }

    public async Task<bool> MoveCategoryAsync(Guid categoryId, bool moveUp, CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        var category = await _db.MenuCategories.SingleOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
        if (category is null) return false;
        var siblings = await _db.MenuCategories.Where(x => x.MenuId == category.MenuId && x.ParentCategoryId == category.ParentCategoryId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        var index = siblings.FindIndex(x => x.Id == categoryId);
        var target = moveUp ? index - 1 : index + 1;
        if (index < 0 || target < 0 || target >= siblings.Count) return true;
        (siblings[index].SortOrder, siblings[target].SortOrder) = (siblings[target].SortOrder, siblings[index].SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<MenuItemDto?> CreateItemAsync(
        Guid categoryId,
        MenuItemInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        EnsureItem(input);
        var currency = await EnsureCurrencyAsync(input.Currency, cancellationToken);
        if (_entitlementService is not null)
            await _entitlementService.EnsureCanCreateMenuItemAsync(cancellationToken);
        if (!await _db.MenuCategories.AnyAsync(x => x.Id == categoryId, cancellationToken))
            return null;

        MenuItem? item = null;
        await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        item = new MenuItem
        {
            TenantId = RequireTenant(),
            MenuCategoryId = categoryId,
            Name = input.Name.Trim(),
            NameEn = input.Name.Trim(),
            NameAr = NullIfEmpty(input.NameAr),
            Description = NullIfEmpty(input.Description),
            DescriptionEn = NullIfEmpty(input.Description),
            DescriptionAr = NullIfEmpty(input.DescriptionAr),
            ProductTypeCode = await EnsureLookupAsync(LookupTypes.ProductType, input.ProductTypeCode, cancellationToken),
            Price = input.Price,
            Currency = currency,
            SortOrder = input.SortOrder,
            IsAvailable = true
        };
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        await ReplaceIngredientsAndImageAsync(item, input, cancellationToken);
        await ReplaceModifiersAsync(item, input.ModifierIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        });
        await WriteAuditAsync("menu-item.created", item!.Id, null, new { item.Name, item.Price, item.Currency, item.IsAvailable }, cancellationToken);
        return await GetItemDtoAsync(item.Id, cancellationToken);
    }

    public async Task<MenuItemDto?> UpdateItemAsync(
        Guid itemId,
        MenuItemInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        EnsureItem(input);
        var currency = await EnsureCurrencyAsync(input.Currency, cancellationToken);
        var item = await _db.MenuItems
            .Include(x => x.Ingredients)
            .Include(x => x.Allergens)
            .Include(x => x.Images)
            .Include(x => x.Modifiers)
            .SingleOrDefaultAsync(x => x.Id == itemId, cancellationToken);
        if (item is null)
            return null;

        var oldValue = new { item.Name, item.Description, item.Price, item.Currency, item.IsAvailable, item.SortOrder, item.MenuCategoryId };
        if (input.CategoryId.HasValue)
        {
            var category = await _db.MenuCategories
                .SingleOrDefaultAsync(x => x.Id == input.CategoryId.Value, cancellationToken);
            if (category is null)
                throw new ArgumentException("The selected category is not available in this tenant.");

            item.MenuCategoryId = category.Id;
        }
        item.Name = input.Name.Trim();
        item.NameEn = input.Name.Trim();
        item.NameAr = NullIfEmpty(input.NameAr);
        item.Description = NullIfEmpty(input.Description);
        item.DescriptionEn = NullIfEmpty(input.Description);
        item.DescriptionAr = NullIfEmpty(input.DescriptionAr);
        item.ProductTypeCode = await EnsureLookupAsync(LookupTypes.ProductType, input.ProductTypeCode, cancellationToken);
        item.Price = input.Price;
        item.Currency = currency;
        item.SortOrder = input.SortOrder;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await ReplaceIngredientsAndImageAsync(item, input, cancellationToken);
        await ReplaceModifiersAsync(item, input.ModifierIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("menu-item.updated", item.Id, oldValue, new { item.Name, item.Description, item.Price, item.Currency, item.IsAvailable, item.SortOrder, item.MenuCategoryId }, cancellationToken);
        return await GetItemDtoAsync(item.Id, cancellationToken);
    }

    public Task<MenuItemDto?> GetItemAsync(Guid itemId, CancellationToken cancellationToken = default) =>
        GetItemDtoAsync(itemId, cancellationToken);

    public async Task<bool> SetItemAvailabilityAsync(
        Guid itemId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        var item = await _db.MenuItems.SingleOrDefaultAsync(x => x.Id == itemId, cancellationToken);
        if (item is null)
            return false;
        var oldValue = new { item.IsAvailable };
        item.IsAvailable = isAvailable;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("menu-item.availability-changed", item.Id, oldValue, new { item.IsAvailable }, cancellationToken);
        return true;
    }

    public async Task<bool> MoveItemAsync(Guid itemId, bool moveUp, CancellationToken cancellationToken = default)
    {
        EnsureTenantWideMenuAccess();
        var item = await _db.MenuItems.SingleOrDefaultAsync(x => x.Id == itemId, cancellationToken);
        if (item is null) return false;
        var siblings = await _db.MenuItems.Where(x => x.MenuCategoryId == item.MenuCategoryId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        var index = siblings.FindIndex(x => x.Id == itemId);
        var target = moveUp ? index - 1 : index + 1;
        if (index < 0 || target < 0 || target >= siblings.Count) return true;
        (siblings[index].SortOrder, siblings[target].SortOrder) = (siblings[target].SortOrder, siblings[index].SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task WriteAuditAsync(
        string action,
        Guid entityId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken) =>
        _auditLogService?.WriteAsync(action, GetAuditEntityType(action), entityId, oldValue, newValue, cancellationToken)
        ?? Task.CompletedTask;

    private static string GetAuditEntityType(string action) =>
        action.StartsWith("menu-item", StringComparison.Ordinal) ? "MenuItem" :
        action.StartsWith("menu-category", StringComparison.Ordinal) ? "MenuCategory" :
        "Menu";

    private async Task ReplaceIngredientsAndImageAsync(
        MenuItem item,
        MenuItemInput input,
        CancellationToken cancellationToken)
    {
        var ingredientIds = input.IngredientIds.Distinct().ToList();
        var allergenIds = input.AllergenIds.Distinct().ToList();
        if (item.Ingredients.Count > 0)
            _db.MenuItemIngredients.RemoveRange(item.Ingredients);
        if (item.Allergens.Count > 0)
            _db.MenuItemAllergens.RemoveRange(item.Allergens);

        var ingredients = await _db.Ingredients
            .Where(x => ingredientIds.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);
        if (ingredients.Count != ingredientIds.Count)
            throw new ArgumentException("One or more selected ingredients are not available in this tenant.");
        foreach (var ingredient in ingredients)
        {
            item.Ingredients.Add(new MenuItemIngredient
            {
                TenantId = RequireTenant(),
                MenuItemId = item.Id,
                IngredientId = ingredient.Id,
                Ingredient = ingredient
            });
        }

        var allergens = await _db.Allergens
            .Where(x => allergenIds.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);
        if (allergens.Count != allergenIds.Count)
            throw new ArgumentException("One or more selected allergens are not available in this tenant.");
        foreach (var allergen in allergens)
        {
            item.Allergens.Add(new MenuItemAllergen
            {
                TenantId = RequireTenant(),
                MenuItemId = item.Id,
                AllergenId = allergen.Id,
                Allergen = allergen
            });
        }

    }

    private async Task<MenuItemDto?> GetItemDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _db.MenuItems
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Ingredients).ThenInclude(x => x.Ingredient)
            .Include(x => x.Allergens).ThenInclude(x => x.Allergen)
            .Include(x => x.Images)
            .Include(x => x.Modifiers)
            .Include(x => x.MenuCategory)
                .ThenInclude(x => x.Menu)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
            return null;

        var dto = ToItemDto(item);
        var overrides = await _db.BranchMenuItemOverrides
            .AsNoTracking()
            .Where(x => x.MenuItemId == item.Id)
            .ToDictionaryAsync(x => x.BranchId, cancellationToken);
        var branchAvailability = await _db.Branches
            .AsNoTracking()
            .Where(x => _db.BranchMenus.Any(b =>
                b.BranchId == x.Id &&
                b.MenuId == item.MenuCategory.MenuId &&
                b.IsActive))
            .OrderBy(x => x.Name)
            .Select(x => new ProductBranchAvailabilityDto(
                x.Id,
                x.Name,
                item.IsAvailable,
                true,
                null))
            .ToListAsync(cancellationToken);
        branchAvailability = branchAvailability
            .Select(x => overrides.TryGetValue(x.BranchId, out var value)
                ? x with
                {
                    IsAvailable = value.IsAvailableOverride ?? x.IsAvailable,
                    IsVisible = value.IsVisibleOverride ?? true,
                    PriceOverride = value.PriceOverride
                }
                : x)
            .ToList();

        return dto with
        {
            CategoryName = item.MenuCategory.Name,
            MenuName = item.MenuCategory.Menu.Name,
            BranchAvailability = branchAvailability
        };
    }

    private async Task<MenuListDto> ToListDtoAsync(Guid id, CancellationToken cancellationToken) =>
        await _db.Menus.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new MenuListDto(x.Id, x.Name, x.Slug, x.IsGlobal, x.Status, x.Categories.Count, x.Categories.SelectMany(c => c.Items).Count(), x.NameAr, x.Description, x.DescriptionAr, x.MenuTypeCode, x.ScopeCode, x.SortOrder, x.BranchMenus.Count(b => b.IsActive)))
            .SingleAsync(cancellationToken);

    private async Task<string> CreateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = Regex.Replace(name.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "menu";
        var slug = baseSlug;
        var suffix = 2;
        while (await _db.Menus.AnyAsync(x => x.Slug == slug, cancellationToken))
            slug = $"{baseSlug}-{suffix++}";
        return slug;
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private void EnsureTenantWideMenuAccess()
    {
        if (_currentUser?.BranchId.HasValue == true)
            throw new UnauthorizedAccessException("Branch-scoped members cannot manage global menu data.");
    }

    private static void EnsureName(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 2)
            throw new ArgumentException($"{label} is required.");
    }

    private static void EnsureItem(MenuItemInput input)
    {
        EnsureName(input.Name, "Item name");
        if (input.Price < 0)
            throw new ArgumentException("Item price cannot be negative.");
        if (string.IsNullOrWhiteSpace(input.Currency) || input.Currency.Trim().Length != 3)
            throw new ArgumentException("Currency must be a three-letter code.");
    }

    private async Task<string> EnsureCurrencyAsync(
        string currency,
        CancellationToken cancellationToken)
    {
        var normalized = currency.Trim().ToUpperInvariant();
        if (_lookupService is not null &&
            !await _lookupService.IsActiveAsync(LookupTypes.Currency, normalized, cancellationToken))
            throw new ArgumentException("Select an active currency from the tenant lookup catalog.");
        return normalized;
    }

    private async Task<string?> EnsureLookupAsync(
        string type,
        string? code,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        var normalized = code.Trim().ToUpperInvariant();
        if (_lookupService is not null &&
            !await _lookupService.IsActiveAsync(type, normalized, cancellationToken))
            throw new ArgumentException("Select an active value from the database lookup catalog.");
        return normalized;
    }

    private async Task ReplaceModifiersAsync(
        MenuItem item,
        IReadOnlyList<Guid>? modifierIds,
        CancellationToken cancellationToken)
    {
        if (item.Modifiers.Count > 0)
            _db.MenuItemModifiers.RemoveRange(item.Modifiers);
        foreach (var modifierId in (modifierIds ?? []).Distinct())
        {
            if (!await _db.Modifiers.AnyAsync(x => x.Id == modifierId && x.IsActive, cancellationToken))
                throw new ArgumentException("One or more selected modifiers are not available in this tenant.");
            item.Modifiers.Add(new MenuItemModifier
            {
                TenantId = RequireTenant(),
                MenuItemId = item.Id,
                ModifierId = modifierId,
                SortOrder = item.Modifiers.Count + 1
            });
        }
    }

    private async Task ReplaceBranchAssignmentsAsync(
        Menu menu,
        IReadOnlyList<Guid>? branchIds,
        CancellationToken cancellationToken)
    {
        var existing = await _db.BranchMenus
            .Where(x => x.MenuId == menu.Id)
            .ToListAsync(cancellationToken);
        if (existing.Count > 0)
            _db.BranchMenus.RemoveRange(existing);

        var requested = (branchIds ?? []).Distinct().ToHashSet();
        if (menu.IsGlobal)
        {
            if (requested.Count > 0)
            {
                var validRequested = await _db.Branches
                    .Where(x => x.IsActive && requested.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);
                if (validRequested.Count != requested.Count)
                    throw new ArgumentException("Every selected branch must belong to this tenant and be active.");
            }

            requested = (await _db.Branches
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken)).ToHashSet();
        }
        else if (requested.Count > 0)
        {
            var valid = await _db.Branches
                .Where(x => x.IsActive && requested.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            if (valid.Count != requested.Count)
                throw new ArgumentException("Every selected branch must belong to this tenant and be active.");
            requested = valid.ToHashSet();
        }

        foreach (var branchId in requested)
            _db.BranchMenus.Add(new BranchMenu
            {
                TenantId = RequireTenant(),
                BranchId = branchId,
                MenuId = menu.Id,
                IsActive = true
            });
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeColor(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var color = value.Trim();
        if (!Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$")) throw new ArgumentException($"Enter a valid six-digit hex value for the {field}.");
        return color.ToUpperInvariant();
    }

    private async Task EnsureBrandingEntitlementAsync(MenuInput input, CancellationToken cancellationToken)
    {
        if (_entitlementService is null ||
            (string.IsNullOrWhiteSpace(input.BrandPrimaryColor) && string.IsNullOrWhiteSpace(input.BrandAccentColor)))
            return;
        if (!await _entitlementService.HasFeatureAsync(FeatureKeys.CustomBranding, cancellationToken))
            throw new EntitlementViolationException("Custom branding is not included in the current plan.", FeatureKeys.CustomBranding);
    }

    private async Task<Guid?> ValidateParentAsync(Guid menuId, Guid? parentCategoryId, Guid? currentCategoryId, CancellationToken cancellationToken)
    {
        if (!parentCategoryId.HasValue) return null;
        if (currentCategoryId == parentCategoryId) throw new ArgumentException("A category cannot be its own parent.");
        var parent = await _db.MenuCategories.SingleOrDefaultAsync(x => x.Id == parentCategoryId.Value && x.MenuId == menuId, cancellationToken);
        if (parent is null) throw new ArgumentException("The selected parent category is not available in this menu.");
        return parent.Id;
    }

    private static MenuItemDto ToItemDto(MenuItem item)
    {
        var ingredients = item.Ingredients.Where(x => x.Ingredient.IsActive).Select(x => x.Ingredient.NameEn ?? x.Ingredient.Name).Distinct().ToList();
        var allergens = item.Allergens.Where(x => x.Allergen.IsActive).Select(x => x.Allergen.NameEn ?? x.Allergen.Name).Distinct().ToList();
        var imageUrl = item.Images
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .FirstOrDefault()?.Url;
        var images = item.Images
            .OrderBy(x => x.SortOrder)
            .ThenByDescending(x => x.IsPrimary)
            .Select(x => new MenuItemImageDto(
                x.Id,
                x.MenuItemId,
                x.Url,
                x.IsPrimary,
                x.SortOrder,
                x.OriginalFileName,
                x.AltText,
                x.ContentType,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.StorageKey))
            .ToList();
        return new MenuItemDto(
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.Currency,
            item.IsAvailable,
            item.SortOrder,
            ingredients,
            allergens,
            imageUrl,
            item.NameAr,
            item.DescriptionAr,
            item.ProductTypeCode,
            item.Modifiers.Select(x => x.ModifierId).ToList(),
            images,
            IngredientIds: item.Ingredients.Where(x => x.Ingredient.IsActive).Select(x => x.IngredientId).ToList(),
            AllergenIds: item.Allergens.Where(x => x.Allergen.IsActive).Select(x => x.AllergenId).ToList());
    }

    private static bool ResolveScope(string? scopeCode, bool legacyIsGlobal) =>
        string.IsNullOrWhiteSpace(scopeCode)
            ? legacyIsGlobal
            : !string.Equals(scopeCode.Trim(), MenuLookupCodes.SelectedBranches, StringComparison.OrdinalIgnoreCase);
}
