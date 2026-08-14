namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record BranchMenuItemOverrideInput(
    Guid BranchId,
    Guid MenuItemId,
    decimal? PriceOverride,
    bool? IsAvailableOverride,
    bool? IsVisibleOverride);

public sealed record BranchMenuItemOverrideDto(
    Guid BranchId,
    Guid MenuItemId,
    decimal EffectivePrice,
    decimal? PriceOverride,
    bool IsAvailable,
    bool? IsAvailableOverride,
    bool IsVisible,
    bool? IsVisibleOverride);

public sealed record BranchSpecificItemInput(
    Guid BranchId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int SortOrder,
    string? NameAr = null,
    string? DescriptionAr = null);

public sealed record BranchSpecificItemDto(
    Guid Id,
    Guid BranchId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsAvailable,
    bool IsVisible,
    int SortOrder,
    string? NameAr = null,
    string? DescriptionAr = null);
