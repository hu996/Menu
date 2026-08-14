using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IAllergenService
{
    Task<AllergenPageDto> GetPageAsync(string? search, bool? isActive = null, string sortBy = "name", bool descending = false, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AllergenDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<AllergenDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AllergenDto> CreateAsync(AllergenInput input, CancellationToken cancellationToken = default);
    Task<AllergenDto?> UpdateAsync(Guid id, AllergenInput input, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
