using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IEntitlementService
{
    Task<EntitlementDto?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<bool> HasFeatureAsync(string featureKey, CancellationToken cancellationToken = default);
    Task EnsureCanCreateBranchAsync(CancellationToken cancellationToken = default);
    Task EnsureCanCreateMenuItemAsync(CancellationToken cancellationToken = default);
    Task EnsureCanAddUserAsync(CancellationToken cancellationToken = default);
}
