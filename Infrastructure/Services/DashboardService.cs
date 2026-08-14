using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IEntitlementService _entitlementService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ICurrentUserContext _currentUser;

    public DashboardService(
        AppDbContext db,
        ITenantContext tenantContext,
        IEntitlementService entitlementService,
        IAnalyticsService analyticsService,
        ICurrentUserContext currentUser)
    {
        _db = db;
        _tenantContext = tenantContext;
        _entitlementService = entitlementService;
        _analyticsService = analyticsService;
        _currentUser = currentUser;
    }

    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
            throw new InvalidOperationException("No tenant is selected.");

        var tenantId = _tenantContext.TenantId.Value;

        var tenant = await _db.Tenants
            .AsNoTracking()
            .SingleAsync(x => x.Id == tenantId, cancellationToken);

        var branchesQuery = _db.Branches.Where(x => x.TenantId == tenantId);
        var menusQuery = _db.Menus.Where(x => x.TenantId == tenantId);
        var categoriesQuery = _db.MenuCategories.Where(x => x.TenantId == tenantId);
        var itemsQuery = _db.MenuItems.Where(x => x.TenantId == tenantId);
        var qrsQuery = _db.QrCodes.Where(x => x.TenantId == tenantId);
        var ordersQuery = _db.Orders.Where(x => x.TenantId == tenantId);
        if (_currentUser.BranchId is Guid branchId)
        {
            branchesQuery = branchesQuery.Where(x => x.Id == branchId);
            menusQuery = menusQuery.Where(x => x.BranchMenus.Any(b => b.BranchId == branchId && b.IsActive));
            categoriesQuery = categoriesQuery.Where(x => x.Menu.BranchMenus.Any(b => b.BranchId == branchId && b.IsActive));
            itemsQuery = itemsQuery.Where(x => x.MenuCategory.Menu.BranchMenus.Any(b => b.BranchId == branchId && b.IsActive));
            qrsQuery = qrsQuery.Where(x => x.BranchId == branchId);
            ordersQuery = ordersQuery.Where(x => x.BranchId == branchId);
        }

        var branches = await branchesQuery.CountAsync(cancellationToken);
        var menus = await menusQuery.CountAsync(cancellationToken);
        var categories = await categoriesQuery.CountAsync(cancellationToken);
        var items = await itemsQuery.CountAsync(cancellationToken);
        var qrs = await qrsQuery.CountAsync(x => x.IsActive, cancellationToken);
        var publishedMenus = await menusQuery.CountAsync(x => x.Status == Domain.Enums.MenuStatus.Published, cancellationToken);
        var draftMenus = await menusQuery.CountAsync(x => x.Status == Domain.Enums.MenuStatus.Draft, cancellationToken);
        var unavailableItems = await itemsQuery.CountAsync(x => !x.IsAvailable, cancellationToken);
        var itemsWithoutImages = await itemsQuery.CountAsync(x => !x.Images.Any(), cancellationToken);
        var pendingOrders = await ordersQuery.CountAsync(x => x.Status == Domain.Enums.OrderStatus.Pending, cancellationToken);
        var onboarding = new OnboardingProgressDto(
            !string.IsNullOrWhiteSpace(tenant.NameEn) && !string.IsNullOrWhiteSpace(tenant.Currency) && !string.IsNullOrWhiteSpace(tenant.DefaultLanguage),
            branches > 0,
            menus > 0,
            categories > 0,
            items > 0,
            publishedMenus > 0,
            qrs > 0);

        var analyticsEnabled = await _entitlementService.HasFeatureAsync(Domain.Constants.FeatureKeys.AdvancedAnalytics, cancellationToken);
        var entitlements = await _entitlementService.GetCurrentAsync(cancellationToken);
        var attention = BuildAttention(
            publishedMenus,
            qrs,
            draftMenus,
            unavailableItems,
            itemsWithoutImages,
            pendingOrders,
            entitlements);
        var recentActivity = new List<DashboardActivityDto>();
        if (!_currentUser.BranchId.HasValue)
        {
            var auditRows = await _db.AuditLogs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(8)
                .Select(x => new
                {
                    x.Action,
                    x.EntityType,
                    x.CreatedAtUtc,
                    x.ActorDisplayName
                })
                .ToListAsync(cancellationToken);
            recentActivity = auditRows
                .Select(x => new DashboardActivityDto(
                    x.Action,
                    x.EntityType,
                    x.CreatedAtUtc,
                    x.ActorDisplayName,
                    ActivityLabel(x.Action)))
                .ToList();
        }

        return new DashboardDto(
            tenant.Name,
            branches,
            menus,
            items,
            qrs,
            pendingOrders,
            publishedMenus,
            draftMenus,
            entitlements,
            analyticsEnabled ? await _analyticsService.GetSummaryAsync(cancellationToken) : new AnalyticsSummaryDto(0, 0, null, [], []),
            onboarding,
            analyticsEnabled,
            tenant.NameAr,
            attention,
            recentActivity);
    }

    private static IReadOnlyList<DashboardAttentionDto> BuildAttention(
        int publishedMenus,
        int activeQrCodes,
        int draftMenus,
        int unavailableItems,
        int itemsWithoutImages,
        int pendingOrders,
        EntitlementDto? entitlements)
    {
        var result = new List<DashboardAttentionDto>();
        if (publishedMenus == 0)
            result.Add(new("Publishing", "Publish your guest menu", "Guests cannot order until at least one menu is published.", "Menus", "Index", "warning", "Menu.View"));
        else if (activeQrCodes == 0)
            result.Add(new("Guest entry point", "Create a guest entry point", "Your menu is published, but no active QR code is available for guests.", "QrCodes", "Index", "warning", "QR.View"));
        if (pendingOrders > 0)
            result.Add(new("Orders", "Orders need attention", $"{pendingOrders} pending {UnitLabel(pendingOrders, "order", "orders")} are waiting for staff action.", "Orders", "Index", "danger", "Orders.View"));
        if (draftMenus > 0)
            result.Add(new("Menu drafts", "Draft menus are waiting", $"{draftMenus} draft {UnitLabel(draftMenus, "menu", "menus")} can be reviewed before publishing.", "Menus", "Index", "neutral", "Menu.View"));
        if (unavailableItems > 0)
            result.Add(new("Availability", "Unavailable products", $"{unavailableItems} {UnitLabel(unavailableItems, "product is", "products are")} currently hidden from guests.", "Products", "Index", "warning", "Product.View"));
        if (itemsWithoutImages > 0)
            result.Add(new("Product media", "Product images are incomplete", $"{itemsWithoutImages} {UnitLabel(itemsWithoutImages, "product is", "products are")} missing a gallery image.", "Products", "Index", "neutral", "Product.View"));
        if (entitlements is not null)
        {
            if (IsAtLimit(entitlements.BranchesUsed, entitlements.MaxBranches))
                result.Add(new("Branches", "Branch limit reached", "Upgrade the plan before adding another branch.", "Billing", "Index", "warning", "Subscription.View"));
            if (IsAtLimit(entitlements.MenuItemsUsed, entitlements.MaxMenuItems))
                result.Add(new("Menu items", "Product limit reached", "Upgrade the plan before adding another product.", "Billing", "Index", "warning", "Subscription.View"));
            if (IsAtLimit(entitlements.UsersUsed, entitlements.MaxUsers))
                result.Add(new("Users", "User limit reached", "Upgrade the plan before inviting another team member.", "Billing", "Index", "warning", "Subscription.View"));
        }
        return result;
    }

    private static bool IsAtLimit(int usage, int limit) => limit > 0 && usage >= limit;

    private static string UnitLabel(int count, string singular, string plural) => count == 1 ? singular : plural;

    private static string ActivityLabel(string action) => action switch
    {
        "order.created" => "Order received",
        "order.status.changed" => "Order status changed",
        "branch.created" => "Branch created",
        "branch.updated" => "Branch updated",
        "menu.created" => "Menu created",
        "menu.updated" => "Menu updated",
        "category.created" => "Category created",
        "category.updated" => "Category updated",
        "item.created" => "Product created",
        "item.updated" => "Product updated",
        "restaurant.updated" => "Restaurant settings updated",
        "payment.initiated" => "Payment initiated",
        _ => "Workspace activity"
    };
}
