using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserContext? _currentUser;

    public AuditLogService(
        AppDbContext db,
        ITenantContext tenantContext,
        ICurrentUserContext? currentUser = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task WriteAsync(
        string action,
        string entityType,
        Guid? entityId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Audit action and entity type are required.");

        var log = new AuditLog
        {
            TenantId = RequireTenant(),
            ActorUserId = _currentUser?.UserId,
            ActorDisplayName = _currentUser?.DisplayName ?? "System",
            Action = action.Trim(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            OldValueJson = Serialize(oldValue),
            NewValueJson = Serialize(newValue)
        };
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var safeTake = Math.Clamp(take, 1, 200);
        return await _db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(safeTake)
            .Select(x => new AuditLogDto(
                x.Id,
                x.ActorUserId,
                x.ActorDisplayName ?? "System",
                x.Action,
                x.EntityType,
                x.EntityId,
                x.OldValueJson,
                x.NewValueJson,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private static string? Serialize(object? value) => value is null
        ? null
        : JsonSerializer.Serialize(value, JsonOptions);
}
