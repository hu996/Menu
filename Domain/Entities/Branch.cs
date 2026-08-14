using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Branch : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string Slug { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? OpeningHours { get; set; }
    public string? BrandPrimaryColorOverride { get; set; }
    public string? BrandAccentColorOverride { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<BranchMenu> BranchMenus { get; set; } = new List<BranchMenu>();
    public ICollection<QrCode> QrCodes { get; set; } = new List<QrCode>();
    public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
}
