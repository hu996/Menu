using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class BranchMenu : TenantEntity
{
    public Guid BranchId { get; set; }
    public Guid MenuId { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch Branch { get; set; } = null!;
    public Menu Menu { get; set; } = null!;
}
