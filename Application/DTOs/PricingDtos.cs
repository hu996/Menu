namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record PricingPreviewRequest(
    string ScopeCode,
    string OperationCode,
    Guid? CategoryId,
    Guid? BranchId,
    IReadOnlyList<Guid> MenuItemIds,
    decimal Value,
    string? Reason);

public sealed record PricingPreviewLine(
    Guid MenuItemId,
    string ItemName,
    string CategoryName,
    Guid? BranchId,
    string? BranchName,
    decimal OldPrice,
    decimal NewPrice,
    decimal ChangeAmount,
    string Currency,
    string? ItemNameAr = null,
    string? CategoryNameAr = null,
    string? BranchNameAr = null);

public sealed record PricingPreviewDto(
    IReadOnlyList<PricingPreviewLine> Lines,
    decimal TotalIncrease,
    decimal TotalDecrease);

public sealed record PricingCatalogDto(
    IReadOnlyList<MenuItemDto> Items,
    IReadOnlyList<PricingCategoryDto> Categories,
    IReadOnlyList<BranchDto> Branches,
    IReadOnlyList<LookupValueDto> Operations,
    IReadOnlyList<LookupValueDto> Scopes);

public sealed record PricingCategoryDto(Guid Id, string Name, string? NameAr = null);

public sealed record PriceHistoryDto(
    Guid Id,
    Guid MenuItemId,
    string ItemName,
    Guid? BranchId,
    string? BranchName,
    decimal PreviousPrice,
    decimal NewPrice,
    string OperationCode,
    decimal? ChangeAmount,
    decimal? ChangePercentage,
    string? Reason,
    Guid? ActorUserId,
    DateTime CreatedAtUtc,
    string? ItemNameAr = null,
    string? BranchNameAr = null);
