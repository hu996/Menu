namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record ProductQuery(
    string? Search = null,
    Guid? MenuId = null,
    Guid? CategoryId = null,
    Guid? BranchId = null,
    bool? IsAvailable = null,
    int Page = 1,
    int PageSize = 25,
    string SortBy = "name",
    bool Descending = false);

public sealed record ProductGridItemDto(
    Guid Id,
    string Name,
    string? NameAr,
    string? Description,
    string? DescriptionAr,
    string? ProductTypeCode,
    decimal Price,
    string Currency,
    bool IsAvailable,
    int SortOrder,
    Guid MenuId,
    string MenuName,
    Guid CategoryId,
    string CategoryName,
    string? PrimaryImageUrl,
    int ImageCount,
    string BranchScope = "All assigned branches");

public sealed record ProductCategoryOptionDto(Guid Id, Guid MenuId, string MenuName, string Name);

public sealed record ProductFilterOptionsDto(
    IReadOnlyList<MenuListDto> Menus,
    IReadOnlyList<ProductCategoryOptionDto> Categories,
    IReadOnlyList<BranchDto> Branches);

public sealed record ProductPageDto(
    IReadOnlyList<ProductGridItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search,
    Guid? MenuId,
    Guid? CategoryId,
    Guid? BranchId,
    bool? IsAvailable,
    string SortBy,
    bool Descending)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record ProductBranchAvailabilityDto(
    Guid BranchId,
    string BranchName,
    bool IsAvailable,
    bool IsVisible,
    decimal? PriceOverride);
