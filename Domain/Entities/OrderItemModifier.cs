using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class OrderItemModifier : TenantEntity
{
    public Guid OrderItemId { get; set; }
    public Guid ModifierOptionId { get; set; }
    public string OptionName { get; set; } = null!;
    public decimal PriceAdjustment { get; set; }

    public OrderItem OrderItem { get; set; } = null!;
    public ModifierOption ModifierOption { get; set; } = null!;
}
