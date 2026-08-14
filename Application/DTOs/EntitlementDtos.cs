using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record EntitlementDto(
    string PlanName,
    SubscriptionStatus Status,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    int BranchesUsed,
    int MaxBranches,
    int MenuItemsUsed,
    int MaxMenuItems,
    int UsersUsed,
    int MaxUsers,
    IReadOnlyDictionary<string, bool> Features)
{
    public bool IsActive => Status is SubscriptionStatus.Trial or SubscriptionStatus.Active
        && (!EndsAtUtc.HasValue || EndsAtUtc.Value > DateTime.UtcNow);

    public bool HasFeature(string featureKey) =>
        Features.TryGetValue(featureKey, out var enabled) && enabled;
}
