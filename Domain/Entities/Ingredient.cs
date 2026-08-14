using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Ingredient : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItemIngredient> MenuItems { get; set; } = new List<MenuItemIngredient>();
}
