using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class ModifierOption : TenantEntity
{
    public Guid ModifierId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public decimal PriceAdjustment { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Modifier Modifier { get; set; } = null!;
}
