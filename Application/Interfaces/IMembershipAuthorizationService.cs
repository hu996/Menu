namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IMembershipAuthorizationService
{
    Task<bool> CanAccessBranchAsync(
        Guid userId,
        Guid branchId,
        CancellationToken cancellationToken = default);
}
