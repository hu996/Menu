using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class QrCode : TenantEntity
{
    public Guid BranchId { get; set; }
    public Guid? TableId { get; set; }
    public string Code { get; set; } = null!;
    public string TargetType { get; set; } = "branch-menu";
    public string? TableLabel { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch Branch { get; set; } = null!;
    public RestaurantTable? Table { get; set; }
}
