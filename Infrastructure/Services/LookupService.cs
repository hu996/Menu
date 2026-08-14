using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class LookupService : ILookupService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogService? _auditLogService;
    private readonly ICurrentUserContext? _currentUser;

    public LookupService(
        AppDbContext db,
        ITenantContext tenantContext,
        IAuditLogService? auditLogService = null,
        ICurrentUserContext? currentUser = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _auditLogService = auditLogService;
        _currentUser = currentUser;
    }

    public async Task<LookupValuePageDto> GetPageAsync(
        string? type,
        string? search,
        bool? isActive = null,
        string sortBy = "type",
        bool descending = false,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var safePageSize = Math.Clamp(pageSize, 10, 100);
        var safePage = Math.Max(page, 1);
        var normalizedType = string.IsNullOrWhiteSpace(type) ? null : NormalizeType(type);
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var query = ApplyValueOwnership(_db.LookupValues.AsNoTracking());

        if (normalizedType is not null)
            query = query.Where(x => x.Type == normalizedType);
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);
        if (normalizedSearch is not null)
            query = query.Where(x =>
                x.Type.Contains(normalizedSearch) ||
                x.Code.Contains(normalizedSearch) ||
                x.NameEn.Contains(normalizedSearch) ||
                (x.NameAr != null && x.NameAr.Contains(normalizedSearch)) ||
                (x.Description != null && x.Description.Contains(normalizedSearch)));

        var total = await query.CountAsync(cancellationToken);
        query = ApplyValueOrdering(query, sortBy, descending);
        var items = await query
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);

        return new LookupValuePageDto(items, safePage, safePageSize, total, normalizedType, normalizedSearch);
    }

    public async Task<IReadOnlyList<LookupValueDto>> GetAllAsync(
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyValueOwnership(_db.LookupValues.AsNoTracking());
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(x => x.Type == NormalizeType(type));

        return await query
            .OrderBy(x => x.Type)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupTypeDto>> GetTypesAsync(
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.LookupTypes.AsNoTracking();
        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await ProjectTypes(query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupTypeDto>> GetTenantManagedTypesAsync(
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.LookupTypes
            .AsNoTracking()
            .Where(x => !x.IsGlobal);
        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await ProjectTypes(query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<LookupTypePageDto> GetTypePageAsync(
        string? search,
        bool? isActive = null,
        string sortBy = "code",
        bool descending = false,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var safePageSize = Math.Clamp(pageSize, 10, 100);
        var safePage = Math.Max(page, 1);
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var query = _db.LookupTypes.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);
        if (normalizedSearch is not null)
            query = query.Where(x =>
                x.Code.Contains(normalizedSearch) ||
                x.NameEn.Contains(normalizedSearch) ||
                (x.NameAr != null && x.NameAr.Contains(normalizedSearch)) ||
                (x.Description != null && x.Description.Contains(normalizedSearch)));

        var total = await query.CountAsync(cancellationToken);
        query = ApplyTypeOrdering(query, sortBy, descending);
        var items = await ProjectTypes(query)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new LookupTypePageDto(items, safePage, safePageSize, total, normalizedSearch);
    }

    public async Task<LookupTypeDto?> GetTypeAsync(Guid id, CancellationToken cancellationToken = default) =>
        await ProjectTypes(_db.LookupTypes.AsNoTracking().Where(x => x.Id == id))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<LookupTypeDto> CreateTypeAsync(
        LookupTypeInput input,
        CancellationToken cancellationToken = default)
    {
        return Task.FromException<LookupTypeDto>(new InvalidOperationException(
            "Lookup types are controlled by the platform catalog. Manage values under an existing lookup type."));
    }

    public async Task<LookupTypeDto?> UpdateTypeAsync(
        Guid id,
        LookupTypeInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.LookupTypes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || (entity.IsGlobal ? !CanManageGlobal() : false))
            return null;

        var normalized = NormalizeTypeInput(input);
        if (!LookupTypeCatalog.IsSupported(normalized.Code) ||
            !string.Equals(entity.Code, normalized.Code, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The lookup type code is controlled by the platform catalog and cannot be changed.");
        if (await _db.LookupTypes.AnyAsync(
                x => x.Id != id && x.Code == normalized.Code,
                cancellationToken))
            throw new InvalidOperationException("That lookup type code already exists.");

        var oldValue = new
        {
            entity.Code,
            entity.NameEn,
            entity.NameAr,
            entity.Description,
            entity.SortOrder,
            entity.IsActive
        };
        var oldCode = entity.Code;
        entity.Code = normalized.Code;
        entity.NameEn = normalized.NameEn;
        entity.NameAr = normalized.NameAr;
        entity.Description = normalized.Description;
        entity.SortOrder = normalized.SortOrder;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        if (!string.Equals(oldCode, normalized.Code, StringComparison.Ordinal))
        {
            var values = await _db.LookupValues
                .Where(x => !x.IsGlobal && x.Type == oldCode)
                .ToListAsync(cancellationToken);
            foreach (var value in values)
            {
                value.Type = normalized.Code;
                value.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        await AuditAsync("lookup-type.updated", "LookupType", entity.Id, oldValue, new
        {
            entity.Code,
            entity.NameEn,
            entity.NameAr,
            entity.Description,
            entity.SortOrder,
            entity.IsActive
        }, cancellationToken);
        return await GetTypeAsync(entity.Id, cancellationToken);
    }

    public async Task<bool> SetTypeActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.LookupTypes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || (entity.IsGlobal ? !CanManageGlobal() : false))
            return false;

        var oldValue = new { entity.IsActive };
        entity.IsActive = isActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await AuditAsync("lookup-type.status-changed", "LookupType", entity.Id, oldValue, new { entity.IsActive }, cancellationToken);
        return true;
    }

    public async Task<bool> MoveTypeAsync(Guid id, bool moveUp, CancellationToken cancellationToken = default)
    {
        var entity = await _db.LookupTypes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || (entity.IsGlobal ? !CanManageGlobal() : false))
            return false;

        var siblings = await _db.LookupTypes
            .Where(x => x.IsGlobal == entity.IsGlobal &&
                        (x.IsGlobal || x.TenantId == RequireTenant()))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Code)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        var index = siblings.FindIndex(x => x.Id == id);
        var neighborIndex = moveUp ? index - 1 : index + 1;
        if (index < 0 || neighborIndex < 0 || neighborIndex >= siblings.Count)
            return false;

        var neighbor = siblings[neighborIndex];
        (entity.SortOrder, neighbor.SortOrder) = (neighbor.SortOrder, entity.SortOrder);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        neighbor.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await AuditAsync("lookup-type.reordered", "LookupType", entity.Id,
            new { SortOrder = neighbor.SortOrder }, new { SortOrder = entity.SortOrder }, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<LookupValueDto>> GetActiveAsync(
        string type,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeType(type);
        return await ApplyValueOwnership(_db.LookupValues
            .AsNoTracking())
            .Where(x => x.Type == normalizedType && x.IsActive &&
                        _db.LookupTypes.Any(t => t.Code == x.Type && t.IsActive && t.IsGlobal == x.IsGlobal))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<LookupValueDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await ApplyValueOwnership(_db.LookupValues
            .AsNoTracking())
            .Where(x => x.Id == id)
            .Select(ToDtoExpression())
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> IsActiveAsync(
        string type,
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeType(type);
        var normalizedCode = NormalizeCode(code);
        return ApplyValueOwnership(_db.LookupValues).AnyAsync(
            x => x.Type == normalizedType && x.Code == normalizedCode && x.IsActive &&
                 _db.LookupTypes.Any(t => t.Code == x.Type && t.IsActive && t.IsGlobal == x.IsGlobal),
            cancellationToken);
    }

    public async Task<LookupValueDto> CreateAsync(
        LookupValueInput input,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(input);
        var type = await _db.LookupTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Code == normalized.Type && x.IsActive, cancellationToken);
        if (type is null)
            throw new InvalidOperationException("Select an active lookup type from the database.");
        if (!type.IsGlobal && type.TenantId != RequireTenant())
            throw new InvalidOperationException("The lookup type is not owned by the current tenant.");
        var codeAllowed = type.IsGlobal
            ? CanManageGlobal() && LookupTypeCatalog.IsGlobalValueCodeAllowed(type.Code, normalized.Code)
            : LookupTypeCatalog.IsValueCodeAllowed(type.Code, normalized.Code);
        if (!codeAllowed)
            throw new InvalidOperationException(type.IsGlobal
                ? "Global lookup values require platform administration and a valid catalog code."
                : "This lookup type only accepts values supported by the platform catalog.");
        if (await ApplyValueOwnership(_db.LookupValues).AnyAsync(
                x => x.Type == normalized.Type && x.Code == normalized.Code,
                cancellationToken))
            throw new InvalidOperationException("That lookup code already exists for this type.");

        var entity = new LookupValue
        {
            TenantId = type.IsGlobal ? Guid.Empty : RequireTenant(),
            IsGlobal = type.IsGlobal,
            Type = normalized.Type,
            Code = normalized.Code,
            NameEn = normalized.NameEn,
            NameAr = normalized.NameAr,
            Description = normalized.Description,
            SortOrder = normalized.SortOrder,
            IsActive = true
        };
        _db.LookupValues.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await AuditAsync("lookup-value.created", "LookupValue", entity.Id, null, new
        {
            entity.Type,
            entity.Code,
            entity.NameEn,
            entity.NameAr,
            entity.Description,
            entity.SortOrder,
            entity.IsActive
        }, cancellationToken);
        return ToDto(entity);
    }

    public async Task<LookupValueDto?> UpdateAsync(
        Guid id,
        LookupValueInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await ApplyValueOwnership(_db.LookupValues)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || (entity.IsGlobal ? !CanManageGlobal() : false))
            return null;

        var normalized = Normalize(input);
        if (!string.Equals(normalized.Type, entity.Type, StringComparison.Ordinal) ||
            !string.Equals(normalized.Code, entity.Code, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Lookup type and code are stable identifiers and cannot be changed after a value is used.");
        var type = await _db.LookupTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Code == normalized.Type && x.IsActive, cancellationToken);
        if (type is null)
            throw new InvalidOperationException("Select an active lookup type from the database.");
        if (type.IsGlobal != entity.IsGlobal || (!type.IsGlobal && type.TenantId != RequireTenant()))
            throw new InvalidOperationException("The lookup type is not owned by the current tenant.");
        var codeAllowed = type.IsGlobal
            ? CanManageGlobal() && LookupTypeCatalog.IsGlobalValueCodeAllowed(type.Code, normalized.Code)
            : LookupTypeCatalog.IsValueCodeAllowed(type.Code, normalized.Code);
        if (!codeAllowed)
            throw new InvalidOperationException(type.IsGlobal
                ? "Global lookup values require platform administration and a valid catalog code."
                : "This lookup type only accepts values supported by the platform catalog.");
        if (await _db.LookupValues.AnyAsync(
                x => x.Id != id && x.IsGlobal == entity.IsGlobal &&
                     (x.IsGlobal || x.TenantId == RequireTenant()) &&
                     x.Type == normalized.Type && x.Code == normalized.Code,
                cancellationToken))
            throw new InvalidOperationException("That lookup code already exists for this type.");

        var oldValue = new
        {
            entity.Type,
            entity.Code,
            entity.NameEn,
            entity.NameAr,
            entity.Description,
            entity.SortOrder,
            entity.IsActive
        };
        entity.Type = normalized.Type;
        entity.Code = normalized.Code;
        entity.NameEn = normalized.NameEn;
        entity.NameAr = normalized.NameAr;
        entity.Description = normalized.Description;
        entity.SortOrder = normalized.SortOrder;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await AuditAsync("lookup-value.updated", "LookupValue", entity.Id, oldValue, new
        {
            entity.Type,
            entity.Code,
            entity.NameEn,
            entity.NameAr,
            entity.Description,
            entity.SortOrder,
            entity.IsActive
        }, cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await ApplyValueOwnership(_db.LookupValues)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || (entity.IsGlobal ? !CanManageGlobal() : false))
            return false;

        var oldValue = new { entity.IsActive };
        entity.IsActive = isActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await AuditAsync("lookup-value.status-changed", "LookupValue", entity.Id, oldValue, new { entity.IsActive }, cancellationToken);
        return true;
    }

    public async Task<bool> MoveValueAsync(Guid id, bool moveUp, CancellationToken cancellationToken = default)
    {
        var entity = await ApplyValueOwnership(_db.LookupValues)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || entity.IsGlobal)
            return false;

        var siblings = await ApplyValueOwnership(_db.LookupValues)
            .Where(x => x.IsGlobal == entity.IsGlobal &&
                        (x.IsGlobal || x.TenantId == RequireTenant()) &&
                        x.Type == entity.Type)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        var index = siblings.FindIndex(x => x.Id == id);
        var neighborIndex = moveUp ? index - 1 : index + 1;
        if (index < 0 || neighborIndex < 0 || neighborIndex >= siblings.Count)
            return false;

        var neighbor = siblings[neighborIndex];
        (entity.SortOrder, neighbor.SortOrder) = (neighbor.SortOrder, entity.SortOrder);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        neighbor.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await AuditAsync("lookup-value.reordered", "LookupValue", entity.Id,
            new { SortOrder = neighbor.SortOrder }, new { SortOrder = entity.SortOrder }, cancellationToken);
        return true;
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private bool CanManageGlobal() => _currentUser?.Role == MembershipRole.PlatformAdmin;

    private IQueryable<LookupTypeDto> ProjectTypes(IQueryable<LookupType> query) => query.Select(x => new LookupTypeDto(
        x.Id,
        x.Code,
        x.NameEn,
        x.NameAr,
        x.Description,
        x.IsGlobal,
        x.IsActive,
        x.SortOrder,
        _db.LookupValues.Count(value => value.Type == x.Code && value.IsGlobal == x.IsGlobal)));

    private IQueryable<LookupValue> ApplyValueOwnership(IQueryable<LookupValue> query) => query.Where(value =>
        _db.LookupTypes.Any(type =>
            type.Code == value.Type &&
            type.IsGlobal == value.IsGlobal &&
            (type.IsGlobal || type.TenantId == value.TenantId)));

    private static IQueryable<LookupValue> ApplyValueOrdering(
        IQueryable<LookupValue> query,
        string sortBy,
        bool descending)
    {
        var key = sortBy.Trim().ToLowerInvariant();
        return key switch
        {
            "code" => descending ? query.OrderByDescending(x => x.Code).ThenBy(x => x.Type) : query.OrderBy(x => x.Code).ThenBy(x => x.Type),
            "english" or "name" => descending ? query.OrderByDescending(x => x.NameEn).ThenBy(x => x.Type) : query.OrderBy(x => x.NameEn).ThenBy(x => x.Type),
            "arabic" => descending ? query.OrderByDescending(x => x.NameAr).ThenBy(x => x.Type) : query.OrderBy(x => x.NameAr).ThenBy(x => x.Type),
            "status" => descending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Type) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Type),
            "order" => descending ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.Type) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.Type),
            _ => descending
                ? query.OrderByDescending(x => x.Type).ThenByDescending(x => x.SortOrder).ThenBy(x => x.NameEn)
                : query.OrderBy(x => x.Type).ThenBy(x => x.SortOrder).ThenBy(x => x.NameEn)
        };
    }

    private static IQueryable<LookupType> ApplyTypeOrdering(
        IQueryable<LookupType> query,
        string sortBy,
        bool descending)
    {
        var key = sortBy.Trim().ToLowerInvariant();
        return key switch
        {
            "english" or "name" => descending ? query.OrderByDescending(x => x.NameEn).ThenBy(x => x.Code) : query.OrderBy(x => x.NameEn).ThenBy(x => x.Code),
            "arabic" => descending ? query.OrderByDescending(x => x.NameAr).ThenBy(x => x.Code) : query.OrderBy(x => x.NameAr).ThenBy(x => x.Code),
            "status" => descending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            "order" => descending ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.Code) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code),
            _ => descending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };
    }

    private static NormalizedLookup Normalize(LookupValueInput input)
    {
        var type = NormalizeType(input.Type);
        var code = NormalizeCode(input.Code);
        var nameEn = input.NameEn?.Trim() ?? string.Empty;
        var nameAr = string.IsNullOrWhiteSpace(input.NameAr) ? null : input.NameAr.Trim();
        var description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        if (type.Length is < 2 or > 64 || code.Length is < 1 or > 64 || nameEn.Length is < 2 or > 160)
            throw new ArgumentException("Lookup type, code, and English name are required and must be within their allowed lengths.");
        if (nameAr?.Length > 160)
            throw new ArgumentException("The Arabic name is too long.");
        if (description?.Length > 500)
            throw new ArgumentException("The lookup description is too long.");
        return new NormalizedLookup(type, code, nameEn, nameAr, description, Math.Max(input.SortOrder, 0));
    }

    private static NormalizedType NormalizeTypeInput(LookupTypeInput input)
    {
        var code = NormalizeType(input.Code);
        var nameEn = input.NameEn?.Trim() ?? string.Empty;
        var nameAr = string.IsNullOrWhiteSpace(input.NameAr) ? null : input.NameAr.Trim();
        var description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        if (code.Length is < 2 or > 64 || nameEn.Length is < 2 or > 160)
            throw new ArgumentException("Lookup type code and English name are required and must be within their allowed lengths.");
        if (nameAr?.Length > 160)
            throw new ArgumentException("The Arabic name is too long.");
        if (description?.Length > 500)
            throw new ArgumentException("The lookup type description is too long.");
        return new NormalizedType(code, nameEn, nameAr, description, Math.Max(input.SortOrder, 0));
    }

    private static string NormalizeType(string? value) => value?.Trim() ?? string.Empty;

    private static string NormalizeCode(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;

    private static LookupValueDto ToDto(LookupValue value) =>
        new(value.Id, value.Type, value.Code, value.NameEn, value.NameAr, value.Description, value.IsActive, value.SortOrder, value.IsGlobal);

    private static Expression<Func<LookupValue, LookupValueDto>> ToDtoExpression() =>
        x => new LookupValueDto(x.Id, x.Type, x.Code, x.NameEn, x.NameAr, x.Description, x.IsActive, x.SortOrder, x.IsGlobal);

    private Task AuditAsync(
        string action,
        string entityType,
        Guid entityId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken) =>
        _auditLogService?.WriteAsync(action, entityType, entityId, oldValue, newValue, cancellationToken) ?? Task.CompletedTask;

    private sealed record NormalizedLookup(
        string Type,
        string Code,
        string NameEn,
        string? NameAr,
        string? Description,
        int SortOrder);

    private sealed record NormalizedType(
        string Code,
        string NameEn,
        string? NameAr,
        string? Description,
        int SortOrder);
}
