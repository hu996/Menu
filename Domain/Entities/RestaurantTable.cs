using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class RestaurantTable : TenantEntity
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch Branch { get; set; } = null!;
    public ICollection<QrCode> QrCodes { get; set; } = new List<QrCode>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
