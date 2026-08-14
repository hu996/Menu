using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class BranchMenuService : IBranchMenuService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserContext? _currentUserContext;
    private readonly ILookupService? _lookupService;
    private readonly IMembershipAuthorizationService _membershipAuthorization;

    public BranchMenuService(
        AppDbContext db,
        ITenantContext tenantContext,
        ICurrentUserContext? currentUserContext = null,
        ILookupService? lookupService = null,
        IMembershipAuthorizationService? membershipAuthorization = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUserContext = currentUserContext;
        _lookupService = lookupService;
        _membershipAuthorization = membershipAuthorization
            ?? throw new InvalidOperationException("Membership authorization is required.");
    }

    public async Task<BranchMenuItemOverrideDto?> UpsertOverrideAsync(
        BranchMenuItemOverrideInput input,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchAccessAsync(input.BranchId, cancellationToken);
        if (input.PriceOverride is < 0)
            throw new ArgumentException("Override price cannot be negative.");

        var branch = await _db.Branches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.BranchId, cancellationToken);
        var item = await _db.MenuItems
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == input.MenuItemId &&
                x.MenuCategory.Menu.BranchMenus.Any(b => b.BranchId == input.BranchId && b.IsActive),
                cancellationToken);
        if (branch is null || item is null)
            return null;

        var overrideEntity = await _db.BranchMenuItemOverrides
            .SingleOrDefaultAsync(x => x.BranchId == input.BranchId && x.MenuItemId == input.MenuItemId, cancellationToken);
        if (overrideEntity is null)
        {
            overrideEntity = new BranchMenuItemOverride
            {
                TenantId = RequireTenant(),
                BranchId = input.BranchId,
                MenuItemId = input.MenuItemId
            };
            _db.BranchMenuItemOverrides.Add(overrideEntity);
        }

        overrideEntity.PriceOverride = input.PriceOverride;
        overrideEntity.IsAvailableOverride = input.IsAvailableOverride;
        overrideEntity.IsVisibleOverride = input.IsVisibleOverride;
        overrideEntity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ToOverrideDto(overrideEntity, item);
    }

    public async Task<IReadOnlyList<BranchMenuItemOverrideDto>> GetOverridesAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchAccessAsync(branchId, cancellationToken);
        var rows = await _db.BranchMenuItemOverrides
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .Join(
                _db.MenuItems.AsNoTracking().Where(item =>
                    item.MenuCategory.Menu.BranchMenus.Any(b => b.BranchId == branchId && b.IsActive)),
                x => x.MenuItemId,
                x => x.Id,
                (overrideEntity, item) => new { overrideEntity, item })
            .ToListAsync(cancellationToken);
        return rows.Select(x => ToOverrideDto(x.overrideEntity, x.item)).ToList();
    }

    public async Task<BranchSpecificItemDto?> CreateBranchItemAsync(
        BranchSpecificItemInput input,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchAccessAsync(input.BranchId, cancellationToken);
        if (string.IsNullOrWhiteSpace(input.Name) || input.Price < 0 || string.IsNullOrWhiteSpace(input.Currency) || input.Currency.Length != 3)
            throw new ArgumentException("Branch item name, non-negative price, and three-letter currency are required.");
        var currency = input.Currency.Trim().ToUpperInvariant();
        if (_lookupService is not null &&
            !await _lookupService.IsActiveAsync(LookupTypes.Currency, currency, cancellationToken))
            throw new ArgumentException("Select an active currency from the tenant lookup catalog.");

        if (!await _db.Branches.AnyAsync(x => x.Id == input.BranchId, cancellationToken) ||
            !await _db.MenuCategories.AnyAsync(x =>
                x.Id == input.CategoryId &&
                x.Menu.BranchMenus.Any(b => b.BranchId == input.BranchId && b.IsActive),
                cancellationToken))
            return null;

        var item = new BranchSpecificMenuItem
        {
            TenantId = RequireTenant(),
            BranchId = input.BranchId,
            CategoryId = input.CategoryId,
            Name = input.Name.Trim(),
            NameEn = input.Name.Trim(),
            NameAr = string.IsNullOrWhiteSpace(input.NameAr) ? null : input.NameAr.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            DescriptionEn = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            DescriptionAr = string.IsNullOrWhiteSpace(input.DescriptionAr) ? null : input.DescriptionAr.Trim(),
            Price = input.Price,
            Currency = currency,
            SortOrder = input.SortOrder
        };
        _db.BranchSpecificMenuItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return ToBranchItemDto(item);
    }

    public async Task<IReadOnlyList<BranchSpecificItemDto>> GetBranchItemsAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchAccessAsync(branchId, cancellationToken);
        return await _db.BranchSpecificMenuItems
            .AsNoTracking()
            .Where(x => x.BranchId == branchId &&
                        x.Category.Menu.BranchMenus.Any(b => b.BranchId == branchId && b.IsActive))
            .OrderBy(x => x.SortOrder)
            .Select(x => new BranchSpecificItemDto(x.Id, x.BranchId, x.CategoryId, x.Name, x.Description, x.Price, x.Currency, x.IsAvailable, x.IsVisible, x.SortOrder, x.NameAr, x.DescriptionAr))
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureBranchAccessAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var userId = _currentUserContext?.UserId;
        if (!userId.HasValue ||
            !await _membershipAuthorization.CanAccessBranchAsync(userId.Value, branchId, cancellationToken))
            throw new UnauthorizedAccessException("The current user is not assigned to this branch.");
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private static BranchMenuItemOverrideDto ToOverrideDto(BranchMenuItemOverride entity, MenuItem item) =>
        new(
            entity.BranchId,
            entity.MenuItemId,
            entity.PriceOverride ?? item.Price,
            entity.PriceOverride,
            entity.IsAvailableOverride ?? item.IsAvailable,
            entity.IsAvailableOverride,
            entity.IsVisibleOverride ?? true,
            entity.IsVisibleOverride);

    private static BranchSpecificItemDto ToBranchItemDto(BranchSpecificMenuItem entity) =>
        new(entity.Id, entity.BranchId, entity.CategoryId, entity.Name, entity.Description, entity.Price, entity.Currency, entity.IsAvailable, entity.IsVisible, entity.SortOrder, entity.NameAr, entity.DescriptionAr);
}
