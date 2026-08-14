using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IBranchMenuService
{
    Task<BranchMenuItemOverrideDto?> UpsertOverrideAsync(
        BranchMenuItemOverrideInput input,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchMenuItemOverrideDto>> GetOverridesAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);

    Task<BranchSpecificItemDto?> CreateBranchItemAsync(
        BranchSpecificItemInput input,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchSpecificItemDto>> GetBranchItemsAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);
}
