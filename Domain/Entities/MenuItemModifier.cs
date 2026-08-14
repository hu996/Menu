using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class MenuItemModifier : TenantEntity
{
    public Guid MenuItemId { get; set; }
    public Guid ModifierId { get; set; }
    public int SortOrder { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
    public Modifier Modifier { get; set; } = null!;
}
