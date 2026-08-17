namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record PublicMenuDto(
    Guid BranchId,
    string RestaurantName,
    string RestaurantSlug,
    string BranchName,
    string BranchSlug,
    IReadOnlyList<PublicMenuSectionDto> Menus,
    string? BrandPrimaryColor = null,
    string? BrandAccentColor = null,
    string? LogoUrl = null,
    string? CoverImageUrl = null,
    string? TableName = null,
    string? TableNameAr = null,
    string? QrCodeCode = null)
{
    public string MenuName => Menus.Count switch
    {
        0 => "Menu",
        1 => Menus[0].MenuName,
        _ => "Menus"
    };
}

public sealed record PublicMenuSectionDto(
    Guid Id,
    string MenuName,
    string? MenuTypeCode,
    string? Description,
    IReadOnlyList<PublicMenuCategoryDto> Categories);

public sealed record PublicMenuCategoryDto(
    string Name,
    IReadOnlyList<PublicMenuItemDto> Items,
    string? ParentName = null);

public sealed record PublicMenuItemDto(
    Guid Id,
    Guid MenuId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsAvailable,
    string? ImageUrl,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<string> Allergens,
    IReadOnlyList<PublicModifierDto> Modifiers,
    IReadOnlyList<PublicMenuImageDto>? Images = null);

public sealed record PublicMenuImageDto(
    string Url,
    string AltText,
    bool IsPrimary,
    int SortOrder);

public sealed record PublicModifierDto(
    string Name,
    bool IsRequired,
    IReadOnlyList<PublicModifierOptionDto> Options,
    string? NameAr = null,
    int MinSelections = 0,
    int MaxSelections = 1);

public sealed record PublicModifierOptionDto(string Name, decimal PriceAdjustment, string? NameAr = null, Guid Id = default);

public sealed record PublicOrderItemDto(
    Guid Id,
    Guid MenuId,
    string Name,
    decimal Price,
    string Currency,
    bool IsAvailable,
    IReadOnlyList<PublicModifierDto> Modifiers,
    string? Description = null,
    string? ImageUrl = null,
    IReadOnlyList<string>? Ingredients = null,
    IReadOnlyList<string>? Allergens = null,
    IReadOnlyList<PublicMenuImageDto>? Images = null);

public sealed record CartLineDto(
    string Key,
    Guid MenuItemId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal ModifierTotal,
    decimal LineTotal,
    string Currency,
    IReadOnlyList<Guid> ModifierOptionIds,
    IReadOnlyList<string> ModifierNames,
    string? ImageUrl = null);

public sealed record CartDto(
    string RestaurantSlug,
    string BranchSlug,
    Guid BranchId,
    IReadOnlyList<CartLineDto> Lines,
    decimal Total,
    string Currency,
    Guid? TableId = null,
    string? TableName = null,
    string? TableNameAr = null,
    Guid? QrCodeId = null,
    string? QrCodeCode = null);

public sealed record OrderReceiptDto(
    Guid Id,
    string OrderNumber,
    string RestaurantName,
    string BranchName,
    decimal Total,
    string Currency,
    string Status,
    IReadOnlyList<CartLineDto> Items,
    string? TableName = null,
    string? TableNameAr = null,
    string? QrCodeCode = null);

public sealed record StaffOrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string CustomerPhone,
    decimal Total,
    string Currency,
    string Status,
    string BranchName,
    DateTime CreatedAtUtc,
    IReadOnlyList<CartLineDto> Items,
    Guid? TableId = null,
    string? TableName = null,
    string? TableNameAr = null,
    Guid? QrCodeId = null,
    string? QrCodeCode = null,
    string? Notes = null);

public sealed record PublicMenuAnalyticsContext(
    Guid BranchId,
    IReadOnlyList<PublicMenuAnalyticsMenuContext> Menus);

public sealed record PublicMenuAnalyticsMenuContext(Guid MenuId, IReadOnlyList<Guid> MenuItemIds);

public sealed record PublicOrderingContextSummary(Guid BranchId);
