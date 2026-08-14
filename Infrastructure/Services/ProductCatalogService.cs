using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class ProductCatalogService : IProductCatalogService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserContext? _currentUser;
    private readonly IAuditLogService? _auditLogService;

    public ProductCatalogService(
        AppDbContext db,
        ICurrentUserContext? currentUser = null,
        IAuditLogService? auditLogService = null)
    {
        _db = db;
        _currentUser = currentUser;
        _auditLogService = auditLogService;
    }

    public async Task<ProductFilterOptionsDto> GetFilterOptionsAsync(
        Guid? restrictedBranchId = null,
        CancellationToken cancellationToken = default)
    {
        restrictedBranchId = ResolveRestrictedBranch(restrictedBranchId);
        var menus = _db.Menus.AsNoTracking();
        var categories = _db.MenuCategories.AsNoTracking();
        var branches = _db.Branches.AsNoTracking().Where(x => x.IsActive);

        if (restrictedBranchId.HasValue)
        {
            menus = menus.Where(x => x.BranchMenus.Any(b => b.BranchId == restrictedBranchId.Value && b.IsActive));
            categories = categories.Where(x => x.Menu.BranchMenus.Any(b => b.BranchId == restrictedBranchId.Value && b.IsActive));
            branches = branches.Where(x => x.Id == restrictedBranchId.Value);
        }

        var menuOptions = await menus
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

        var categoryOptions = await categories
            .OrderBy(x => x.Menu.SortOrder)
            .ThenBy(x => x.Menu.Name)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ProductCategoryOptionDto(x.Id, x.MenuId, x.Menu.Name, x.Name))
            .ToListAsync(cancellationToken);

        var branchOptions = await branches
            .OrderBy(x => x.Name)
            .Select(x => new BranchDto(
                x.Id,
                x.Name,
                x.Slug,
                x.Address,
                x.Phone,
                x.IsActive,
                x.NameAr,
                x.Latitude,
                x.Longitude,
                x.OpeningHours,
                x.BrandPrimaryColorOverride,
                x.BrandAccentColorOverride))
            .ToListAsync(cancellationToken);

        return new ProductFilterOptionsDto(menuOptions, categoryOptions, branchOptions);
    }

    public async Task<ProductPageDto> GetPageAsync(
        ProductQuery query,
        Guid? restrictedBranchId = null,
        CancellationToken cancellationToken = default)
    {
        restrictedBranchId = ResolveRestrictedBranch(restrictedBranchId);
        var safePage = Math.Max(1, query.Page);
        var safePageSize = Math.Clamp(query.PageSize, 10, 100);
        var effectiveBranchId = restrictedBranchId ?? query.BranchId;
        var normalizedSearch = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var products = _db.MenuItems.AsNoTracking().AsQueryable();

        if (query.MenuId.HasValue)
            products = products.Where(x => x.MenuCategory.MenuId == query.MenuId.Value);
        if (query.CategoryId.HasValue)
            products = products.Where(x => x.MenuCategoryId == query.CategoryId.Value);
        if (effectiveBranchId.HasValue)
        {
            products = products.Where(x => x.MenuCategory.Menu.BranchMenus.Any(b =>
                b.BranchId == effectiveBranchId.Value && b.IsActive));
        }
        if (normalizedSearch is not null)
        {
            products = products.Where(x =>
                x.Name.Contains(normalizedSearch) ||
                (x.NameEn != null && x.NameEn.Contains(normalizedSearch)) ||
                (x.NameAr != null && x.NameAr.Contains(normalizedSearch)) ||
                (x.Description != null && x.Description.Contains(normalizedSearch)) ||
                (x.DescriptionAr != null && x.DescriptionAr.Contains(normalizedSearch)));
        }
        if (query.IsAvailable.HasValue)
        {
            if (effectiveBranchId.HasValue)
            {
                products = products.Where(x =>
                    ((_db.BranchMenuItemOverrides
                        .Where(o => o.MenuItemId == x.Id && o.BranchId == effectiveBranchId.Value)
                        .Select(o => o.IsAvailableOverride)
                        .FirstOrDefault()) ?? x.IsAvailable) == query.IsAvailable.Value);
            }
            else
            {
                products = products.Where(x => x.IsAvailable == query.IsAvailable.Value);
            }
        }

        var total = await products.CountAsync(cancellationToken);
        products = ApplySort(products, query.SortBy, query.Descending);
        var entities = await products
            .Include(x => x.MenuCategory)
                .ThenInclude(x => x.Menu)
                    .ThenInclude(x => x.BranchMenus)
                        .ThenInclude(x => x.Branch)
            .Include(x => x.Images)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        var overrides = effectiveBranchId.HasValue
            ? await _db.BranchMenuItemOverrides.AsNoTracking()
                .Where(x => x.BranchId == effectiveBranchId.Value && entities.Select(i => i.Id).Contains(x.MenuItemId))
                .ToDictionaryAsync(x => x.MenuItemId, cancellationToken)
            : [];

        var rows = entities.Select(item =>
        {
            overrides.TryGetValue(item.Id, out var branchOverride);
            var isAvailable = branchOverride?.IsAvailableOverride ?? item.IsAvailable;
            var primary = item.Images
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .FirstOrDefault()?.Url;
            var branchScope = item.MenuCategory.Menu.IsGlobal
                ? "All active branches"
                : string.Join(", ", item.MenuCategory.Menu.BranchMenus
                    .Where(x => x.IsActive)
                    .Select(x => x.Branch.Name)
                    .OrderBy(x => x));
            return new ProductGridItemDto(
                item.Id,
                item.Name,
                item.NameAr,
                item.Description,
                item.DescriptionAr,
                item.ProductTypeCode,
                branchOverride?.PriceOverride ?? item.Price,
                item.Currency,
                isAvailable,
                item.SortOrder,
                item.MenuCategory.MenuId,
                item.MenuCategory.Menu.Name,
                item.MenuCategoryId,
                item.MenuCategory.Name,
                primary,
                item.Images.Count,
                string.IsNullOrWhiteSpace(branchScope) ? "No active branch" : branchScope);
        }).ToList();

        return new ProductPageDto(
            rows,
            safePage,
            safePageSize,
            total,
            normalizedSearch,
            query.MenuId,
            query.CategoryId,
            effectiveBranchId,
            query.IsAvailable,
            query.SortBy,
            query.Descending);
    }

    public async Task<int> SetAvailabilityAsync(
        IReadOnlyList<Guid> itemIds,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser?.BranchId.HasValue == true)
            throw new UnauthorizedAccessException("Branch-scoped members cannot change global product availability.");

        var ids = itemIds.Distinct().ToList();
        if (ids.Count == 0)
            return 0;

        var items = await _db.MenuItems.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.IsAvailable = isAvailable;
            item.UpdatedAtUtc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(cancellationToken);
        if (_auditLogService is not null)
        {
            await _auditLogService.WriteAsync(
                "menu-items.bulk-availability-changed",
                "MenuItem",
                null,
                null,
                new { Count = items.Count, IsAvailable = isAvailable },
                cancellationToken);
        }
        return items.Count;
    }

    private static IQueryable<Domain.Entities.MenuItem> ApplySort(
        IQueryable<Domain.Entities.MenuItem> query,
        string? sortBy,
        bool descending)
    {
        var normalized = sortBy?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "price" => descending ? query.OrderByDescending(x => x.Price).ThenBy(x => x.Name) : query.OrderBy(x => x.Price).ThenBy(x => x.Name),
            "category" => descending ? query.OrderByDescending(x => x.MenuCategory.Name).ThenBy(x => x.Name) : query.OrderBy(x => x.MenuCategory.Name).ThenBy(x => x.Name),
            "sort" => descending ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.Name) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name),
            "availability" => descending ? query.OrderByDescending(x => x.IsAvailable).ThenBy(x => x.Name) : query.OrderBy(x => x.IsAvailable).ThenBy(x => x.Name),
            _ => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };
    }

    private Guid? ResolveRestrictedBranch(Guid? requestedBranchId) =>
        _currentUser?.BranchId is Guid scopedBranchId
            ? scopedBranchId
            : requestedBranchId;
}
