namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record ModifierOptionDto(Guid Id, string Name, decimal PriceAdjustment, int SortOrder, bool IsActive, string? NameAr = null);

public sealed record ModifierDto(
    Guid Id,
    string Name,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    bool IsActive,
    IReadOnlyList<ModifierOptionDto> Options,
    string? NameAr = null);

public sealed record ModifierPageDto(
    IReadOnlyList<ModifierDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record ModifierOptionInput(string Name, decimal PriceAdjustment, int SortOrder, string? NameAr = null, bool IsActive = true);

public sealed record ModifierInput(
    string Name,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    IReadOnlyList<ModifierOptionInput> Options,
    string? NameAr = null,
    bool IsActive = true);
