using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Currency { get; set; }
    public string DefaultLanguage { get; set; } = null!;
    public string? BrandPrimaryColor { get; set; }
    public string? BrandAccentColor { get; set; }
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trial;
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Menu> Menus { get; set; } = new List<Menu>();
    public ICollection<TenantBrandingImage> BrandingImages { get; set; } = new List<TenantBrandingImage>();
}
