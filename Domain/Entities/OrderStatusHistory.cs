using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class OrderStatusHistory : TenantEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorDisplayName { get; set; }

    public Order Order { get; set; } = null!;
}
