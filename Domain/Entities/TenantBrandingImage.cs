using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public sealed class TenantBrandingImage : TenantEntity
{
    public TenantBrandingKind Kind { get; set; }
    public string Url { get; set; } = null!;
    public string StorageKey { get; set; } = null!;
    public string? OriginalFileName { get; set; }
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public string? AltText { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
