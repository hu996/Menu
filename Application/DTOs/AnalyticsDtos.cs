namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record AnalyticsMenuDto(Guid MenuId, string MenuName, int Views, string? MenuNameAr = null);

public sealed record AnalyticsItemDto(Guid MenuItemId, string ItemName, int Views, string? ItemNameAr = null);

public sealed record AnalyticsBranchDto(Guid BranchId, string BranchName, int Scans, int MenuViews, string? BranchNameAr = null);

public sealed record AnalyticsSummaryDto(
    int TotalScans,
    int TodayScans,
    AnalyticsMenuDto? MostViewedMenu,
    IReadOnlyList<AnalyticsItemDto> MostViewedItems,
    IReadOnlyList<AnalyticsBranchDto> BranchComparison);
