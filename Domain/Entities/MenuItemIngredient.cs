using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class MenuItemIngredient : TenantEntity
{
    public Guid MenuItemId { get; set; }
    public Guid IngredientId { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
