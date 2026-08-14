using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class PricingService : IPricingService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ILookupService _lookupService;
    private readonly IAuditLogService? _auditLogService;
    private readonly ICurrentUserContext? _currentUser;

    public PricingService(
        AppDbContext db,
        ITenantContext tenantContext,
        ILookupService lookupService,
        IAuditLogService? auditLogService = null,
        ICurrentUserContext? currentUser = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _lookupService = lookupService;
        _auditLogService = auditLogService;
        _currentUser = currentUser;
    }

    public async Task<PricingCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var itemRows = await _db.MenuItems
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.NameAr, x.Description, x.Price, x.Currency, x.IsAvailable, x.SortOrder })
            .ToListAsync(cancellationToken);
        var items = itemRows
            .Select(x => new MenuItemDto(x.Id, x.Name, x.Description, x.Price, x.Currency, x.IsAvailable, x.SortOrder, [], [], null, x.NameAr))
            .ToList();
        var branches = await _db.Branches
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BranchDto(x.Id, x.Name, x.Slug, x.Address, x.Phone, x.IsActive, x.NameAr, x.Latitude, x.Longitude, x.OpeningHours, x.BrandPrimaryColorOverride, x.BrandAccentColorOverride))
            .ToListAsync(cancellationToken);
        var categories = await _db.MenuCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new PricingCategoryDto(x.Id, x.Name, x.NameAr))
            .ToListAsync(cancellationToken);
        var operations = await _lookupService.GetActiveAsync(LookupTypes.PricingOperation, cancellationToken);
        var scopes = await _lookupService.GetActiveAsync(LookupTypes.PricingScope, cancellationToken);
        return new PricingCatalogDto(items, categories, branches, operations, scopes);
    }

    public Task<PricingPreviewDto> PreviewAsync(
        PricingPreviewRequest request,
        CancellationToken cancellationToken = default) =>
        BuildPreviewAsync(request, cancellationToken);

    public async Task<PricingPreviewDto> ApplyAsync(
        PricingPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        // Snapshot rule: MenuItem.Price is the recoverable base price. A branch
        // operation writes BranchMenuItemOverride.PriceOverride. Effective price
        // is therefore Branch override ?? base product price.
        PricingPreviewDto? preview = null;
        await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        preview = await BuildPreviewAsync(request, cancellationToken);
        if (preview.Lines.Count == 0)
            throw new ArgumentException("Select at least one product affected by the pricing operation.");

        foreach (var line in preview.Lines)
        {
            var item = await _db.MenuItems.SingleOrDefaultAsync(x => x.Id == line.MenuItemId, cancellationToken)
                ?? throw new InvalidOperationException("A selected menu item is no longer available.");

            if (line.BranchId.HasValue)
            {
                var branchOverride = await _db.BranchMenuItemOverrides
                    .SingleOrDefaultAsync(
                        x => x.BranchId == line.BranchId.Value && x.MenuItemId == line.MenuItemId,
                        cancellationToken);
                if (branchOverride is null)
                {
                    branchOverride = new BranchMenuItemOverride
                    {
                        TenantId = RequireTenant(),
                        BranchId = line.BranchId.Value,
                        MenuItemId = line.MenuItemId
                    };
                    _db.BranchMenuItemOverrides.Add(branchOverride);
                }
                branchOverride.PriceOverride = line.NewPrice;
                branchOverride.UpdatedAtUtc = DateTime.UtcNow;
            }
            else
            {
                item.Price = line.NewPrice;
                item.UpdatedAtUtc = DateTime.UtcNow;
            }

            _db.PriceHistories.Add(new PriceHistory
            {
                TenantId = RequireTenant(),
                MenuItemId = line.MenuItemId,
                BranchId = line.BranchId,
                PreviousPrice = line.OldPrice,
                NewPrice = line.NewPrice,
                OperationCode = NormalizeCode(request.OperationCode),
                ChangeAmount = line.ChangeAmount,
                ChangePercentage = line.OldPrice == 0 ? null : line.ChangeAmount / line.OldPrice * 100,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                ActorUserId = _currentUser?.UserId
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        if (_auditLogService is not null)
        {
            await _auditLogService.WriteAsync(
                "pricing.bulk-applied",
                "Pricing",
                null,
                null,
                new
                {
                    Scope = NormalizeCode(request.ScopeCode),
                    Operation = NormalizeCode(request.OperationCode),
                    Count = preview.Lines.Count,
                    request.Value,
                    request.Reason
                },
                cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        });
        return preview!;
    }

    public async Task<IReadOnlyList<PriceHistoryDto>> GetHistoryAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var safeTake = Math.Clamp(take, 1, 500);
        return await _db.PriceHistories
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(safeTake)
            .Select(x => new PriceHistoryDto(
                x.Id,
                x.MenuItemId,
                x.MenuItem.Name,
                x.BranchId,
                x.Branch == null ? null : x.Branch.Name,
                x.PreviousPrice,
                x.NewPrice,
                x.OperationCode,
                x.ChangeAmount,
                x.ChangePercentage,
                x.Reason,
                x.ActorUserId,
                x.CreatedAtUtc,
                x.MenuItem.NameAr,
                x.Branch == null ? null : x.Branch.NameAr))
            .ToListAsync(cancellationToken);
    }

    private async Task<PricingPreviewDto> BuildPreviewAsync(
        PricingPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var scopeCode = NormalizeCode(request.ScopeCode);
        var operationCode = NormalizeCode(request.OperationCode);
        if (!await _lookupService.IsActiveAsync(LookupTypes.PricingScope, scopeCode, cancellationToken))
            throw new ArgumentException("Select an active pricing scope.");
        if (!await _lookupService.IsActiveAsync(LookupTypes.PricingOperation, operationCode, cancellationToken))
            throw new ArgumentException("Select an active pricing operation.");
        if (request.Value < 0)
            throw new ArgumentException("Pricing value cannot be negative.");
        if (operationCode is PricingLookupCodes.PercentageIncrease or PricingLookupCodes.PercentageDecrease && request.Value > 100)
            throw new ArgumentException("Percentage changes must be between 0 and 100.");

        if (request.BranchId.HasValue && !await _db.Branches.AnyAsync(x => x.Id == request.BranchId.Value && x.IsActive, cancellationToken))
            throw new ArgumentException("The selected branch is not available.");

        var query = _db.MenuItems.AsNoTracking();
        if (scopeCode == PricingLookupCodes.Product)
        {
            var ids = request.MenuItemIds.Distinct().ToList();
            if (ids.Count == 0)
                throw new ArgumentException("Select at least one product.");
            var tenantProductCount = await _db.MenuItems
                .CountAsync(x => ids.Contains(x.Id), cancellationToken);
            if (tenantProductCount != ids.Count)
                throw new ArgumentException("Every selected product must belong to the current tenant.");
            query = query.Where(x => ids.Contains(x.Id));
        }
        else if (scopeCode == PricingLookupCodes.Category)
        {
            if (!request.CategoryId.HasValue || !await _db.MenuCategories.AnyAsync(x => x.Id == request.CategoryId.Value, cancellationToken))
                throw new ArgumentException("Select a valid category.");
            query = query.Where(x => x.MenuCategoryId == request.CategoryId.Value);
        }
        else if (scopeCode == PricingLookupCodes.Branch && !request.BranchId.HasValue)
        {
            throw new ArgumentException("Select a branch for branch pricing.");
        }

        // A branch price is meaningful only for products whose menu is
        // assigned to that branch. This also prevents a tenant-wide admin
        // request from creating branch overrides for unrelated catalog rows.
        if (request.BranchId.HasValue)
        {
            query = query.Where(x => x.MenuCategory.Menu.BranchMenus.Any(x =>
                x.BranchId == request.BranchId.Value && x.IsActive));
        }

        var items = await query
            .Include(x => x.MenuCategory)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
        var overrides = request.BranchId.HasValue
            ? await _db.BranchMenuItemOverrides
                .AsNoTracking()
                .Where(x => x.BranchId == request.BranchId.Value && items.Select(i => i.Id).Contains(x.MenuItemId))
                .ToDictionaryAsync(x => x.MenuItemId, cancellationToken)
            : new Dictionary<Guid, BranchMenuItemOverride>();
        var branch = request.BranchId.HasValue
            ? await _db.Branches.Where(x => x.Id == request.BranchId.Value).Select(x => new { x.Name, x.NameAr }).SingleAsync(cancellationToken)
            : null;

        var lines = items.Select(item =>
        {
            var current = request.BranchId.HasValue && overrides.TryGetValue(item.Id, out var branchOverride) && branchOverride.PriceOverride.HasValue
                ? branchOverride.PriceOverride.Value
                : item.Price;
            var updated = Calculate(current, operationCode, request.Value);
            return new PricingPreviewLine(
                item.Id,
                item.NameEn ?? item.Name,
                item.MenuCategory.NameEn ?? item.MenuCategory.Name,
                request.BranchId,
                branch?.Name,
                current,
                updated,
                Math.Round(updated - current, 2),
                item.Currency,
                item.NameAr,
                item.MenuCategory.NameAr,
                branch?.NameAr);
        }).ToList();

        return new PricingPreviewDto(
            lines,
            lines.Where(x => x.ChangeAmount > 0).Sum(x => x.ChangeAmount),
            lines.Where(x => x.ChangeAmount < 0).Sum(x => Math.Abs(x.ChangeAmount)));
    }

    private static decimal Calculate(decimal current, string operationCode, decimal value)
    {
        var calculated = operationCode switch
        {
            PricingLookupCodes.PercentageIncrease => current * (1 + value / 100),
            PricingLookupCodes.PercentageDecrease => current * (1 - value / 100),
            PricingLookupCodes.FixedIncrease => current + value,
            PricingLookupCodes.FixedDecrease => current - value,
            PricingLookupCodes.SetExact => value,
            _ => throw new ArgumentException("The selected pricing operation is not supported.")
        };
        if (calculated < 0)
            throw new ArgumentException("A pricing operation cannot produce a negative price.");
        return Math.Round(calculated, 2, MidpointRounding.AwayFromZero);
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private static string NormalizeCode(string value) => value?.Trim().ToUpperInvariant() ?? string.Empty;
}
