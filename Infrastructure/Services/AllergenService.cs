using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class AllergenService : IAllergenService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogService? _auditLogService;

    public AllergenService(AppDbContext db, ITenantContext tenantContext, IAuditLogService? auditLogService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _auditLogService = auditLogService;
    }

    public async Task<AllergenPageDto> GetPageAsync(string? search, bool? isActive = null, string sortBy = "name", bool descending = false, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 10, 100);
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var query = _db.Allergens.AsNoTracking();
        if (normalizedSearch is not null)
            query = query.Where(x => x.Name.Contains(normalizedSearch));
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var ordered = sortBy.ToLowerInvariant() switch
        {
            "status" => descending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Name),
            "arabic" => descending ? query.OrderByDescending(x => x.NameAr).ThenBy(x => x.Name) : query.OrderBy(x => x.NameAr).ThenBy(x => x.Name),
            _ => descending ? query.OrderByDescending(x => x.Name).ThenBy(x => x.NameAr) : query.OrderBy(x => x.Name).ThenBy(x => x.NameAr)
        };
        var items = await ordered
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => new AllergenDto(x.Id, x.NameEn ?? x.Name, x.IsActive, x.NameAr))
            .ToListAsync(cancellationToken);
        return new AllergenPageDto(items, safePage, safePageSize, total, normalizedSearch);
    }

    public async Task<IReadOnlyList<AllergenDto>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _db.Allergens.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new AllergenDto(x.Id, x.NameEn ?? x.Name, x.IsActive, x.NameAr)).ToListAsync(cancellationToken);

    public Task<AllergenDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Allergens.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AllergenDto(x.Id, x.NameEn ?? x.Name, x.IsActive, x.NameAr))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<AllergenDto> CreateAsync(AllergenInput input, CancellationToken cancellationToken = default)
    {
        var name = Normalize(input.Name);
        var nameAr = Normalize(input.NameAr);
        EnsureName(name);
        if (await _db.Allergens.AnyAsync(x => x.Name == name, cancellationToken))
            throw new InvalidOperationException("That allergen already exists.");

        var allergen = new Allergen { TenantId = RequireTenant(), Name = name, NameEn = name, NameAr = nameAr, IsActive = true };
        _db.Allergens.Add(allergen);
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("allergen.created", allergen.Id, null, new { allergen.Name }, cancellationToken);
        return ToDto(allergen);
    }

    public async Task<AllergenDto?> UpdateAsync(Guid id, AllergenInput input, CancellationToken cancellationToken = default)
    {
        var name = Normalize(input.Name);
        var nameAr = Normalize(input.NameAr);
        EnsureName(name);
        var allergen = await _db.Allergens.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (allergen is null)
            return null;
        if (await _db.Allergens.AnyAsync(x => x.Id != id && x.Name == name, cancellationToken))
            throw new InvalidOperationException("That allergen already exists.");

        var oldValue = new { allergen.Name, allergen.NameAr, allergen.IsActive };
        allergen.Name = name;
        allergen.NameEn = name;
        allergen.NameAr = nameAr;
        allergen.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("allergen.updated", allergen.Id, oldValue, new { allergen.Name, allergen.NameAr, allergen.IsActive }, cancellationToken);
        return ToDto(allergen);
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var allergen = await _db.Allergens.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (allergen is null)
            return false;
        var oldValue = new { allergen.IsActive };
        allergen.IsActive = isActive;
        allergen.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("allergen.status-changed", allergen.Id, oldValue, new { allergen.IsActive }, cancellationToken);
        return true;
    }

    private Task WriteAuditAsync(string action, Guid entityId, object? oldValue, object? newValue, CancellationToken cancellationToken) =>
        _auditLogService?.WriteAsync(action, "Allergen", entityId, oldValue, newValue, cancellationToken) ?? Task.CompletedTask;

    private Guid RequireTenant() => _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");
    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
    private static void EnsureName(string name)
    {
        if (name.Length is < 2 or > 160)
            throw new ArgumentException("Allergen name must be between 2 and 160 characters.");
    }
    private static AllergenDto ToDto(Allergen allergen) => new(allergen.Id, allergen.NameEn ?? allergen.Name, allergen.IsActive, allergen.NameAr);
}
