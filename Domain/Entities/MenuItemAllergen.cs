using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class MenuItemAllergen : TenantEntity
{
    public Guid MenuItemId { get; set; }
    public Guid AllergenId { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
    public Allergen Allergen { get; set; } = null!;
}
