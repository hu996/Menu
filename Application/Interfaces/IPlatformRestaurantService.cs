using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IPlatformRestaurantService
{
    Task<IReadOnlyList<PlatformRestaurantDto>> GetAllAsync(
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<PlatformRestaurantDetailsDto?> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default);

    Task<PlatformRestaurantProvisioningResult> ProvisionAsync(
        PlatformRestaurantProvisioningInput input,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        Guid tenantId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
