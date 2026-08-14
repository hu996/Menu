using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface ITableService
{
    Task<IReadOnlyList<RestaurantTableDto>> GetForBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<RestaurantTableDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RestaurantTableDto?> CreateAsync(Guid branchId, RestaurantTableInput input, CancellationToken cancellationToken = default);
    Task<RestaurantTableDto?> UpdateAsync(Guid id, RestaurantTableInput input, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
