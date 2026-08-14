using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class OrderItem : TenantEntity
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
    public ICollection<OrderItemModifier> Modifiers { get; set; } = new List<OrderItemModifier>();
}
