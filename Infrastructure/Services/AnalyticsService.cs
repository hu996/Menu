using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserContext _currentUser;

    public AnalyticsService(AppDbContext db, ITenantContext tenantContext, ICurrentUserContext currentUser)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task TrackPublicMenuViewAsync(
        Guid branchId,
        IReadOnlyCollection<Guid> menuIds,
        bool isQrScan,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        var device = NormalizeDevice(userAgent);
        foreach (var menuId in menuIds.Where(x => x != Guid.Empty).Distinct())
        {
            _db.AnalyticsEvents.Add(new AnalyticsEvent
            {
                TenantId = tenantId,
                EventType = AnalyticsEventType.MenuView,
                BranchId = branchId,
                MenuId = menuId,
                Device = device
            });
        }

        if (isQrScan)
        {
            _db.AnalyticsEvents.Add(new AnalyticsEvent
            {
                TenantId = tenantId,
                EventType = AnalyticsEventType.QrScan,
                BranchId = branchId,
                Device = device
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task TrackMenuItemViewAsync(
        Guid branchId,
        Guid menuId,
        Guid menuItemId,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        if (menuId == Guid.Empty || menuItemId == Guid.Empty)
            return;

        _db.AnalyticsEvents.Add(new AnalyticsEvent
        {
            TenantId = RequireTenant(),
            EventType = AnalyticsEventType.MenuItemView,
            BranchId = branchId,
            MenuId = menuId,
            MenuItemId = menuItemId,
            Device = NormalizeDevice(userAgent)
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        RequireTenant();
        var events = _db.AnalyticsEvents.AsNoTracking();
        if (_currentUser.BranchId is Guid branchId)
            events = events.Where(x => x.BranchId == branchId);
        var totalScans = await events.CountAsync(x => x.EventType == AnalyticsEventType.QrScan, cancellationToken);
        var today = DateTime.UtcNow.Date;
        var todayScans = await events.CountAsync(
            x => x.EventType == AnalyticsEventType.QrScan && x.CreatedAtUtc >= today,
            cancellationToken);

        var menuCount = await events
            .Where(x => x.EventType == AnalyticsEventType.MenuView && x.MenuId.HasValue)
            .GroupBy(x => x.MenuId!.Value)
            .Select(x => new { MenuId = x.Key, Views = x.Count() })
            .OrderByDescending(x => x.Views)
            .FirstOrDefaultAsync(cancellationToken);
        AnalyticsMenuDto? mostViewedMenu = null;
        if (menuCount is not null)
        {
            mostViewedMenu = await _db.Menus
                .Where(x => x.Id == menuCount.MenuId)
                .Select(x => new AnalyticsMenuDto(x.Id, x.Name, menuCount.Views, x.NameAr))
                .SingleOrDefaultAsync(cancellationToken);
        }

        var itemCounts = await events
            .Where(x => x.EventType == AnalyticsEventType.MenuItemView && x.MenuItemId.HasValue)
            .GroupBy(x => x.MenuItemId!.Value)
            .Select(x => new { MenuItemId = x.Key, Views = x.Count() })
            .OrderByDescending(x => x.Views)
            .Take(5)
            .ToListAsync(cancellationToken);
        var itemIds = itemCounts.Select(x => x.MenuItemId).ToList();
        var itemNames = await _db.MenuItems
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.NameAr })
            .ToDictionaryAsync(x => x.Id, x => new { x.Name, x.NameAr }, cancellationToken);
        var mostViewedItems = itemCounts
            .Where(x => itemNames.ContainsKey(x.MenuItemId))
            .Select(x => new AnalyticsItemDto(x.MenuItemId, itemNames[x.MenuItemId].Name, x.Views, itemNames[x.MenuItemId].NameAr))
            .ToList();

        var branchStats = await events
            .GroupBy(x => x.BranchId)
            .Select(x => new
            {
                BranchId = x.Key,
                Scans = x.Count(e => e.EventType == AnalyticsEventType.QrScan),
                MenuViews = x.Count(e => e.EventType == AnalyticsEventType.MenuView)
            })
            .OrderByDescending(x => x.Scans)
            .ThenByDescending(x => x.MenuViews)
            .ToListAsync(cancellationToken);
        var branchIds = branchStats.Select(x => x.BranchId).ToList();
        var branchNames = await _db.Branches
            .Where(x => branchIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.NameAr })
            .ToDictionaryAsync(x => x.Id, x => new { x.Name, x.NameAr }, cancellationToken);
        var branchComparison = branchStats
            .Where(x => branchNames.ContainsKey(x.BranchId))
            .Select(x => new AnalyticsBranchDto(x.BranchId, branchNames[x.BranchId].Name, x.Scans, x.MenuViews, branchNames[x.BranchId].NameAr))
            .ToList();

        return new AnalyticsSummaryDto(totalScans, todayScans, mostViewedMenu, mostViewedItems, branchComparison);
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private static string NormalizeDevice(string? userAgent)
    {
        var value = userAgent?.ToLowerInvariant() ?? string.Empty;
        if (value.Contains("ipad") || value.Contains("tablet"))
            return "tablet";
        if (value.Contains("mobile") || value.Contains("android") || value.Contains("iphone"))
            return "mobile";
        return string.IsNullOrWhiteSpace(value) ? "unknown" : "desktop";
    }
}
