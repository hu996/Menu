using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IPublicMenuService
{
    Task<PublicMenuDto?> GetAsync(
        string restaurantSlug,
        string branchSlug,
        string? language = null,
        CancellationToken cancellationToken = default);

    Task<PublicMenuDto?> GetPreviewAsync(
        Guid menuId,
        Guid branchId,
        string? language = null,
        CancellationToken cancellationToken = default);

    Task<PublicOrderingContextSummary?> GetOrderingContextAsync(
        string restaurantSlug,
        string branchSlug,
        CancellationToken cancellationToken = default);
}
