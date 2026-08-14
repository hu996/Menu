using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IUserManagementService
{
    Task<UserMembershipPageDto> GetPageAsync(
        string? search,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionOptionDto>> GetPermissionOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<UserMembershipDto?> GetAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default);

    Task<UserMembershipDto> CreateAsync(
        UserMembershipInput input,
        CancellationToken cancellationToken = default);

    Task<UserMembershipDto?> UpdateAsync(
        Guid membershipId,
        UserMembershipUpdateInput input,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        Guid membershipId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
