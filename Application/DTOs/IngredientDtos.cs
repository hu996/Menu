namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record IngredientDto(
    Guid Id,
    string Name,
    bool IsActive,
    string? NameAr = null);

public sealed record IngredientPageDto(
    IReadOnlyList<IngredientDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record IngredientInput(string Name, string? NameAr = null);
