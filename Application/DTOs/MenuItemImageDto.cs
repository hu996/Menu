namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record MenuItemImageDto(
    Guid Id,
    Guid MenuItemId,
    string Url,
    bool IsPrimary,
    int SortOrder,
    string? OriginalFileName,
    string? AltText = null,
    string? ContentType = null,
    DateTime CreatedAtUtc = default,
    DateTime? UpdatedAtUtc = null,
    string? StorageKey = null);
