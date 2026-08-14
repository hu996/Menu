using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class MenuCategory : TenantEntity
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? ClassificationCode { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public Menu Menu { get; set; } = null!;
    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
