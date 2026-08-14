namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record BranchQuery(
    string? Search = null,
    bool? IsActive = null,
    string SortBy = "name",
    bool Descending = false,
    int Page = 1,
    int PageSize = 10);

public sealed record BranchPageDto(
    IReadOnlyList<BranchDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search,
    bool? IsActive,
    string SortBy,
    bool Descending)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
