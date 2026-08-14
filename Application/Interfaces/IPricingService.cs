using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IPricingService
{
    Task<PricingCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default);

    Task<PricingPreviewDto> PreviewAsync(
        PricingPreviewRequest request,
        CancellationToken cancellationToken = default);

    Task<PricingPreviewDto> ApplyAsync(
        PricingPreviewRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PriceHistoryDto>> GetHistoryAsync(
        int take = 100,
        CancellationToken cancellationToken = default);
}
