namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record AllergenDto(Guid Id, string Name, bool IsActive, string? NameAr = null);

public sealed record AllergenPageDto(
    IReadOnlyList<AllergenDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record AllergenInput(string Name, string? NameAr = null);
