namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    Guid? ActorUserId,
    string ActorDisplayName,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? OldValueJson,
    string? NewValueJson,
    DateTime CreatedAtUtc);
