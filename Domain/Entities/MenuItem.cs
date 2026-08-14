using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class MenuItem : TenantEntity
{
    public Guid MenuCategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? ProductTypeCode { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = null!;
    public bool IsAvailable { get; set; } = true;
    public int SortOrder { get; set; }

    public MenuCategory MenuCategory { get; set; } = null!;
    public ICollection<MenuItemImage> Images { get; set; } = new List<MenuItemImage>();
    public ICollection<MenuItemIngredient> Ingredients { get; set; } = new List<MenuItemIngredient>();
    public ICollection<MenuItemAllergen> Allergens { get; set; } = new List<MenuItemAllergen>();
    public ICollection<MenuItemModifier> Modifiers { get; set; } = new List<MenuItemModifier>();
}
