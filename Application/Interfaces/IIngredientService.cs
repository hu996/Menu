using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IIngredientService
{
    Task<IngredientPageDto> GetPageAsync(string? search, bool? isActive = null, string sortBy = "name", bool descending = false, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IngredientDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IngredientDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IngredientDto> CreateAsync(IngredientInput input, CancellationToken cancellationToken = default);
    Task<IngredientDto?> UpdateAsync(Guid id, IngredientInput input, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
