using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class PublicMenuService : IPublicMenuService
{
    private readonly AppDbContext _db;

    public PublicMenuService(AppDbContext db)
    {
        _db = db;
    }

    public Task<PublicMenuDto?> GetAsync(
        string restaurantSlug,
        string branchSlug,
        string? language = null,
        CancellationToken cancellationToken = default) =>
        BuildAsync(restaurantSlug, branchSlug, null, null, language, false, cancellationToken);

    public Task<PublicMenuDto?> GetPreviewAsync(
        Guid menuId,
        Guid branchId,
        string? language = null,
        CancellationToken cancellationToken = default) =>
        BuildAsync(null, null, menuId, branchId, language, true, cancellationToken);

    private async Task<PublicMenuDto?> BuildAsync(
        string? restaurantSlug,
        string? branchSlug,
        Guid? requestedMenuId,
        Guid? requestedBranchId,
        string? language,
        bool allowNonPublishedPreview,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Tenant? tenant;
        Domain.Entities.Branch? branch;

        if (requestedMenuId.HasValue && requestedBranchId.HasValue)
        {
            var requestedMenu = await _db.Menus
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == requestedMenuId.Value, cancellationToken);
            if (requestedMenu is null)
                return null;

            tenant = await _db.Tenants
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == requestedMenu.TenantId, cancellationToken);
            branch = await _db.Branches
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == requestedBranchId.Value &&
                         x.TenantId == requestedMenu.TenantId &&
                         x.IsActive,
                    cancellationToken);
        }
        else
        {
            tenant = await _db.Tenants
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Slug == restaurantSlug && x.IsActive, cancellationToken);
            branch = tenant is null
                ? null
                : await _db.Branches
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        x => x.TenantId == tenant.Id && x.Slug == branchSlug && x.IsActive,
                        cancellationToken);
        }

        if (tenant is null || branch is null)
            return null;

        var selectedLanguage = string.IsNullOrWhiteSpace(language)
            ? tenant.DefaultLanguage
            : language;

        var assignedMenus = _db.BranchMenus
            .AsNoTracking()
            .Where(x => x.BranchId == branch.Id && x.IsActive)
            .Join(
                _db.Menus.AsNoTracking().Where(x => allowNonPublishedPreview || x.Status == Domain.Enums.MenuStatus.Published),
                x => x.MenuId,
                x => x.Id,
                (_, menu) => new { menu.Id, menu.SortOrder });
        if (requestedMenuId.HasValue)
            assignedMenus = assignedMenus.Where(x => x.Id == requestedMenuId.Value);

        var menuIds = await assignedMenus
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        if (menuIds.Count == 0)
            return null;

        var menus = await _db.Menus
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
                        .ThenInclude(x => x.Modifier)
                            .ThenInclude(x => x.Options)
            .Where(x => menuIds.Contains(x.Id) &&
                        (allowNonPublishedPreview || x.Status == Domain.Enums.MenuStatus.Published))
            .ToListAsync(cancellationToken);
        if (menus.Count == 0)
            return null;

        var overrides = await _db.BranchMenuItemOverrides
            .AsNoTracking()
            .Where(x => x.BranchId == branch.Id)
            .ToDictionaryAsync(x => x.MenuItemId, cancellationToken);

        var categoryIds = menus.SelectMany(x => x.Categories).Select(x => x.Id).ToList();
        var branchSpecificItems = await _db.BranchSpecificMenuItems
            .AsNoTracking()
            .Where(x => x.BranchId == branch.Id &&
                        categoryIds.Contains(x.CategoryId) &&
                        x.IsVisible)
            .ToListAsync(cancellationToken);
        var branchItemsByCategory = branchSpecificItems.ToLookup(x => x.CategoryId);

        var sections = menus
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(menu =>
            {
                var names = menu.Categories.ToDictionary(
                    x => x.Id,
                    x => Localized(x.Name, x.NameEn, x.NameAr, selectedLanguage) ?? string.Empty);
                var categories = menu.Categories
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.ParentCategoryId.HasValue ? 1 : 0)
                    .ThenBy(x => x.ParentCategoryId)
                    .ThenBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .Select(category =>
                    {
                        var globalItems = category.Items
                            .OrderBy(x => x.SortOrder)
                            .Select(item => ToItem(
                                item,
                                overrides.GetValueOrDefault(item.Id),
                                tenant.Id,
                                tenant.Slug,
                                selectedLanguage))
                            .Where(item => item is not null)
                            .Select(item => item!);
                        var branchItems = branchItemsByCategory[category.Id]
                            .OrderBy(x => x.SortOrder)
                            .Select(x => ToBranchItem(x, selectedLanguage));
                        return new PublicMenuCategoryDto(
                            names[category.Id],
                            globalItems.Concat(branchItems).ToList(),
                            category.ParentCategoryId.HasValue
                                ? names.GetValueOrDefault(category.ParentCategoryId.Value)
                                : null);
                    })
                    .Where(category => category.Items.Count > 0)
                    .ToList();
                return new PublicMenuSectionDto(
                    menu.Id,
                    Localized(menu.Name, menu.NameEn, menu.NameAr, selectedLanguage) ?? string.Empty,
                    menu.MenuTypeCode,
                    Localized(menu.Description, menu.Description, menu.DescriptionAr, selectedLanguage),
                    categories);
            })
            .ToList();

        return new PublicMenuDto(
            branch.Id,
            Localized(tenant.Name, tenant.NameEn, tenant.NameAr, selectedLanguage) ?? string.Empty,
            tenant.Slug,
            Localized(branch.Name, branch.NameEn, branch.NameAr, selectedLanguage) ?? string.Empty,
            branch.Slug,
            sections,
            branch.BrandPrimaryColorOverride ?? tenant.BrandPrimaryColor,
            branch.BrandAccentColorOverride ?? tenant.BrandAccentColor,
            ToPublicImageUrl(tenant.LogoUrl, tenant.Id, tenant.Slug),
            ToPublicImageUrl(tenant.CoverImageUrl, tenant.Id, tenant.Slug));
    }

    public async Task<PublicOrderingContextSummary?> GetOrderingContextAsync(
        string restaurantSlug,
        string branchSlug,
        CancellationToken cancellationToken = default)
    {
        return await _db.BranchMenus
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.Branch.IsActive &&
                        x.Branch.Slug == branchSlug &&
                        x.Branch.Tenant.IsActive &&
                        x.Branch.Tenant.Slug == restaurantSlug &&
                        x.Menu.Status == Domain.Enums.MenuStatus.Published)
            .Select(x => new PublicOrderingContextSummary(x.BranchId))
            .Distinct()
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static PublicMenuItemDto? ToItem(
        Domain.Entities.MenuItem item,
        Domain.Entities.BranchMenuItemOverride? overrideEntity,
        Guid tenantId,
        string tenantSlug,
        string? language)
    {
        if (overrideEntity?.IsVisibleOverride == false)
            return null;

        // A global disable is authoritative; branch overrides can only further
        // restrict availability and cannot make a disabled product orderable.
        var isAvailable = item.IsAvailable && (overrideEntity?.IsAvailableOverride ?? true);

        var ingredients = item.Ingredients
            .Where(x => x.Ingredient.IsActive)
            .Select(x => Localized(x.Ingredient.Name, x.Ingredient.NameEn, x.Ingredient.NameAr, language) ?? x.Ingredient.Name)
            .Distinct()
            .ToList();
        var allergens = item.Allergens
            .Where(x => x.Allergen.IsActive)
            .Select(x => Localized(x.Allergen.Name, x.Allergen.NameEn, x.Allergen.NameAr, language) ?? x.Allergen.Name)
            .Distinct()
            .ToList();
        var images = item.Images
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .Select(image => new PublicMenuImageDto(
                ToPublicImageUrl(image.Url, tenantId, tenantSlug) ?? image.Url,
                string.IsNullOrWhiteSpace(image.AltText) ? item.Name : image.AltText,
                image.IsPrimary,
                image.SortOrder))
            .ToList();
        var imageUrl = images.Count > 0 ? images[0].Url : null;
        var modifiers = item.Modifiers
            .Where(x => x.Modifier.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PublicModifierDto(
                Localized(x.Modifier.Name, x.Modifier.NameEn, x.Modifier.NameAr, language) ?? x.Modifier.Name,
                x.Modifier.IsRequired,
                x.Modifier.Options
                    .Where(option => option.IsActive)
                    .OrderBy(option => option.SortOrder)
                    .Select(option => new PublicModifierOptionDto(
                        Localized(option.Name, option.NameEn, option.NameAr, language) ?? option.Name,
                        option.PriceAdjustment,
                        option.NameAr,
                        option.Id))
                    .ToList(),
                x.Modifier.NameAr,
                x.Modifier.MinSelections,
                x.Modifier.MaxSelections))
            .ToList();
        return new PublicMenuItemDto(
            item.Id,
            item.MenuCategory.MenuId,
            Localized(item.Name, item.NameEn, item.NameAr, language) ?? string.Empty,
            Localized(item.Description, item.DescriptionEn, item.DescriptionAr, language),
            overrideEntity?.PriceOverride ?? item.Price,
            item.Currency,
            isAvailable,
            imageUrl,
            ingredients,
            allergens,
            modifiers,
            images);
    }

    private static PublicMenuItemDto ToBranchItem(Domain.Entities.BranchSpecificMenuItem item, string? language) => new(
        Guid.Empty,
        Guid.Empty,
        Localized(item.Name, item.NameEn, item.NameAr, language) ?? item.Name,
        Localized(item.Description, item.DescriptionEn, item.DescriptionAr, language),
        item.Price,
        item.Currency,
        item.IsAvailable,
        null,
        [],
        [],
        []);

    private static string? Localized(string? fallback, string? english, string? arabic, string? language) =>
        string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase)
            ? arabic ?? english ?? fallback
            : english ?? fallback;

    private static string? ToPublicImageUrl(string? url, Guid tenantId, string tenantSlug) =>
        string.IsNullOrWhiteSpace(url)
            ? null
            : url.StartsWith($"/media/{tenantId:D}/", StringComparison.OrdinalIgnoreCase)
                ? $"/media/{tenantSlug}/{(url.Contains("/branding/", StringComparison.OrdinalIgnoreCase) ? "branding" : "menu-items")}/{Uri.EscapeDataString(Path.GetFileName(url))}"
                : url;
}
