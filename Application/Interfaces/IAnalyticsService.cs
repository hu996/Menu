using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IAnalyticsService
{
    Task TrackQrScanAsync(Guid branchId, string? userAgent, CancellationToken cancellationToken = default);
    Task TrackMenuViewAsync(Guid branchId, Guid menuId, string? userAgent, CancellationToken cancellationToken = default);
    Task TrackMenuItemViewsAsync(Guid branchId, Guid menuId, IReadOnlyCollection<Guid> menuItemIds, string? userAgent, CancellationToken cancellationToken = default);
    Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
