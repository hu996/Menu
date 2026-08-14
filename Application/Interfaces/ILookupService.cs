using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface ILookupService
{
    Task<LookupValuePageDto> GetPageAsync(
        string? type,
        string? search,
        bool? isActive = null,
        string sortBy = "type",
        bool descending = false,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupValueDto>> GetAllAsync(
        string? type = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupTypeDto>> GetTypesAsync(
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupTypeDto>> GetTenantManagedTypesAsync(
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    Task<LookupTypePageDto> GetTypePageAsync(
        string? search,
        bool? isActive = null,
        string sortBy = "code",
        bool descending = false,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<LookupTypeDto?> GetTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LookupTypeDto> CreateTypeAsync(LookupTypeInput input, CancellationToken cancellationToken = default);

    Task<LookupTypeDto?> UpdateTypeAsync(Guid id, LookupTypeInput input, CancellationToken cancellationToken = default);

    Task<bool> SetTypeActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);

    Task<bool> MoveTypeAsync(Guid id, bool moveUp, CancellationToken cancellationToken = default);

    Task<bool> MoveValueAsync(Guid id, bool moveUp, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupValueDto>> GetActiveAsync(
        string type,
        CancellationToken cancellationToken = default);

    Task<LookupValueDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(
        string type,
        string code,
        CancellationToken cancellationToken = default);

    Task<LookupValueDto> CreateAsync(
        LookupValueInput input,
        CancellationToken cancellationToken = default);

    Task<LookupValueDto?> UpdateAsync(
        Guid id,
        LookupValueInput input,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);
}
