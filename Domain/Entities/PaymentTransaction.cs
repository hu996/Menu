using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class PaymentTransaction : TenantEntity
{
    public Guid? SubscriptionId { get; set; }
    public Guid? RequestedPlanId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string ProviderReference { get; set; } = null!;
    public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;
    public DateTime? CompletedAtUtc { get; set; }

    public Subscription? Subscription { get; set; }
}
