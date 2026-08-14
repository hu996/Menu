using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class LookupValue : TenantEntity
{
    public bool IsGlobal { get; set; }
    public string Type { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
