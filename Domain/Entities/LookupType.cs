using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class LookupType : TenantEntity
{
    public bool IsGlobal { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
