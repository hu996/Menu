using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IPlanManagementService
{
    Task<IReadOnlyList<PlanDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PlanDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PlanDto> CreateAsync(PlanManagementInput input, CancellationToken cancellationToken = default);
    Task<PlanDto?> UpdateAsync(Guid id, PlanManagementInput input, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
