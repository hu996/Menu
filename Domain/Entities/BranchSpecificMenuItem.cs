using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class BranchSpecificMenuItem : TenantEntity
{
    public Guid BranchId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = null!;
    public bool IsAvailable { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }

    public Branch Branch { get; set; } = null!;
    public MenuCategory Category { get; set; } = null!;
}
