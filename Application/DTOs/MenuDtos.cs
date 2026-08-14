using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record MenuListDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsGlobal,
    MenuStatus Status,
    int Categories,
    int Items,
    string? NameAr = null,
    string? Description = null,
    string? DescriptionAr = null,
    string? MenuTypeCode = null,
    string? ScopeCode = null,
    int SortOrder = 0,
    int AssignedBranches = 0);

public sealed record MenuDetailsDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsGlobal,
    MenuStatus Status,
    IReadOnlyList<MenuCategoryDto> Categories,
    string? NameAr = null,
    string? MenuTypeCode = null,
    string? ScopeCode = null,
    IReadOnlyList<Guid>? BranchIds = null,
    string? Description = null,
    string? DescriptionAr = null,
    string? BrandPrimaryColor = null,
    string? BrandAccentColor = null,
    int SortOrder = 0,
    IReadOnlyList<MenuBranchDto>? AssignedBranches = null);

public sealed record MenuBranchDto(Guid Id, string Name, string Slug, bool IsActive);

public sealed record MenuCategoryDto(
    Guid Id,
    string Name,
    int SortOrder,
    IReadOnlyList<MenuItemDto> Items,
    string? NameAr = null,
    string? Description = null,
    string? DescriptionAr = null,
    string? ClassificationCode = null,
    Guid? ParentCategoryId = null,
    Guid? MenuId = null);

public sealed record MenuItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsAvailable,
    int SortOrder,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<string> Allergens,
    string? ImageUrl,
    string? NameAr = null,
    string? DescriptionAr = null,
    string? ProductTypeCode = null,
    IReadOnlyList<Guid>? ModifierIds = null,
    IReadOnlyList<MenuItemImageDto>? Images = null,
    string? CategoryName = null,
    string? MenuName = null,
    IReadOnlyList<ProductBranchAvailabilityDto>? BranchAvailability = null,
    IReadOnlyList<Guid>? IngredientIds = null,
    IReadOnlyList<Guid>? AllergenIds = null);

public sealed record MenuInput(
    string Name,
    bool IsGlobal,
    string? NameAr = null,
    string? MenuTypeCode = null,
    string? ScopeCode = null,
    IReadOnlyList<Guid>? BranchIds = null,
    string? Description = null,
    string? DescriptionAr = null,
    string? BrandPrimaryColor = null,
    string? BrandAccentColor = null,
    int SortOrder = 0);

public sealed record MenuCategoryInput(
    string Name,
    int SortOrder,
    string? NameAr = null,
    string? Description = null,
    string? DescriptionAr = null,
    string? ClassificationCode = null,
    Guid? ParentCategoryId = null);

public sealed record MenuItemInput(
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int SortOrder,
    IReadOnlyList<Guid> IngredientIds,
    IReadOnlyList<Guid> AllergenIds,
    string? NameAr = null,
    string? DescriptionAr = null,
    string? ProductTypeCode = null,
    IReadOnlyList<Guid>? ModifierIds = null,
    Guid? CategoryId = null);
