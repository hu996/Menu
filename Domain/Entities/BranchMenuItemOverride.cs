using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class BranchMenuItemOverride : TenantEntity
{
    public Guid BranchId { get; set; }
    public Guid MenuItemId { get; set; }
    public decimal? PriceOverride { get; set; }
    public bool? IsAvailableOverride { get; set; }
    public bool? IsVisibleOverride { get; set; }

    public Branch Branch { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
}
