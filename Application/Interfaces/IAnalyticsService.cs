using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IAnalyticsService
{
    Task TrackPublicMenuViewAsync(Guid branchId, IReadOnlyCollection<Guid> menuIds, bool isQrScan, string? userAgent, CancellationToken cancellationToken = default);
    Task TrackMenuItemViewAsync(Guid branchId, Guid menuId, Guid menuItemId, string? userAgent, CancellationToken cancellationToken = default);
    Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
