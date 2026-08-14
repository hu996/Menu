using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IAuditLogService
{
    Task WriteAsync(
        string action,
        string entityType,
        Guid? entityId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(
        int take = 50,
        CancellationToken cancellationToken = default);
}
