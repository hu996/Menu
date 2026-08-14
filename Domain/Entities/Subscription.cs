using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Subscription : TenantEntity
{
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTime StartsAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndsAtUtc { get; set; }
    public string? PaymentProvider { get; set; }
    public string? ExternalSubscriptionId { get; set; }

    public Plan Plan { get; set; } = null!;
}
