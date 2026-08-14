using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Allergen : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItemAllergen> MenuItems { get; set; } = new List<MenuItemAllergen>();
}
