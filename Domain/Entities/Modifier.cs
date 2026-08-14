using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Modifier : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public bool IsRequired { get; set; }
    public int MinSelections { get; set; }
    public int MaxSelections { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public ICollection<ModifierOption> Options { get; set; } = new List<ModifierOption>();
    public ICollection<MenuItemModifier> MenuItems { get; set; } = new List<MenuItemModifier>();
}
