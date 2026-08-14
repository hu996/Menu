using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Menu : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string Slug { get; set; } = null!;
    public string? MenuTypeCode { get; set; }
    public string? ScopeCode { get; set; }
    public string? BrandPrimaryColor { get; set; }
    public string? BrandAccentColor { get; set; }
    public int SortOrder { get; set; }
    public bool IsGlobal { get; set; }
    public MenuStatus Status { get; set; } = MenuStatus.Draft;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<MenuCategory> Categories { get; set; } = new List<MenuCategory>();
    public ICollection<BranchMenu> BranchMenus { get; set; } = new List<BranchMenu>();
}
