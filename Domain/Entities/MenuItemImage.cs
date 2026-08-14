using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class MenuItemImage : TenantEntity
{
    public Guid MenuItemId { get; set; }
    public string Url { get; set; } = null!;
    public string? StorageKey { get; set; }
    public bool IsPrimary { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public string? AltText { get; set; }
    public int SortOrder { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
}
