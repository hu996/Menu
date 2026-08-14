using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IModifierService
{
    Task<ModifierPageDto> GetPageAsync(string? search, bool? isActive = null, string sortBy = "name", bool descending = false, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModifierDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ModifierDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ModifierDto> CreateAsync(ModifierInput input, CancellationToken cancellationToken = default);
    Task<ModifierDto?> UpdateAsync(Guid id, ModifierInput input, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
