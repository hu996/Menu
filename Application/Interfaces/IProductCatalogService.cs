using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IProductCatalogService
{
    Task<ProductFilterOptionsDto> GetFilterOptionsAsync(
        Guid? restrictedBranchId = null,
        CancellationToken cancellationToken = default);

    Task<ProductPageDto> GetPageAsync(
        ProductQuery query,
        Guid? restrictedBranchId = null,
        CancellationToken cancellationToken = default);

    Task<int> SetAvailabilityAsync(
        IReadOnlyList<Guid> itemIds,
        bool isAvailable,
        CancellationToken cancellationToken = default);
}
