using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

/// <summary>
/// Data-driven feature entitlement for a plan. A null limit means the feature is
/// boolean-only; numeric limits remain owned by the plan so they can be changed
/// without changing application code.
/// </summary>
public class PlanFeature : BaseEntity
{
    public Guid PlanId { get; set; }
    public string FeatureKey { get; set; } = null!;
    public bool Enabled { get; set; } = true;
    public int? LimitValue { get; set; }

    public Plan Plan { get; set; } = null!;
}
