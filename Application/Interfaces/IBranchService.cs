using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IBranchService
{
    Task<IReadOnlyList<BranchDto>> GetAllAsync(
        Guid? restrictedBranchId = null,
        CancellationToken cancellationToken = default);

    Task<BranchPageDto> GetPageAsync(
        BranchQuery query,
        Guid? restrictedBranchId = null,
        CancellationToken cancellationToken = default);

    Task<BranchDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BranchDto> CreateAsync(BranchInput input, CancellationToken cancellationToken = default);

    Task<BranchDto?> UpdateAsync(Guid id, BranchInput input, CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
