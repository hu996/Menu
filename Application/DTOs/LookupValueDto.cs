namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record LookupValueDto(
    Guid Id,
    string Type,
    string Code,
    string NameEn,
    string? NameAr,
    string? Description,
    bool IsActive,
    int SortOrder,
    bool IsGlobal = false);

public sealed record LookupValueInput(
    string Type,
    string Code,
    string NameEn,
    string? NameAr,
    string? Description,
    int SortOrder);

public sealed record LookupTypeDto(
    Guid Id,
    string Code,
    string NameEn,
    string? NameAr,
    string? Description,
    bool IsGlobal,
    bool IsActive,
    int SortOrder,
    int ValueCount);

public sealed record LookupTypeInput(
    string Code,
    string NameEn,
    string? NameAr,
    string? Description,
    int SortOrder);

public sealed record LookupTypePageDto(
    IReadOnlyList<LookupTypeDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record LookupValuePageDto(
    IReadOnlyList<LookupValueDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Type,
    string? Search)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
