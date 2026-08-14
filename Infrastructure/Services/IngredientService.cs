using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class IngredientService : IIngredientService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogService? _auditLogService;

    public IngredientService(AppDbContext db, ITenantContext tenantContext, IAuditLogService? auditLogService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _auditLogService = auditLogService;
    }

    public async Task<IngredientPageDto> GetPageAsync(string? search, bool? isActive = null, string sortBy = "name", bool descending = false, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 10, 100);
        var query = _db.Ingredients.AsNoTracking();
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
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
            .Select(x => new IngredientDto(x.Id, x.NameEn ?? x.Name, x.IsActive, x.NameAr))
            .ToListAsync(cancellationToken);
        return new IngredientPageDto(items, safePage, safePageSize, total, normalizedSearch);
    }

    public async Task<IReadOnlyList<IngredientDto>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _db.Ingredients.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new IngredientDto(x.Id, x.NameEn ?? x.Name, x.IsActive, x.NameAr)).ToListAsync(cancellationToken);

    public async Task<IngredientDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Ingredients.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new IngredientDto(x.Id, x.NameEn ?? x.Name, x.IsActive, x.NameAr))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IngredientDto> CreateAsync(IngredientInput input, CancellationToken cancellationToken = default)
    {
        var name = Normalize(input.Name);
        var nameAr = Normalize(input.NameAr);
        if (name.Length is < 2 or > 160)
            throw new ArgumentException("Ingredient name must be between 2 and 160 characters.");
        if (await _db.Ingredients.AnyAsync(x => x.Name == name, cancellationToken))
            throw new InvalidOperationException("That ingredient already exists.");

        var ingredient = new Ingredient { TenantId = RequireTenant(), Name = name, NameEn = name, NameAr = nameAr, IsActive = true };
        _db.Ingredients.Add(ingredient);
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("ingredient.created", ingredient.Id, null, new { ingredient.Name, ingredient.NameAr }, cancellationToken);
        return ToDto(ingredient);
    }

    public async Task<IngredientDto?> UpdateAsync(Guid id, IngredientInput input, CancellationToken cancellationToken = default)
    {
        var name = Normalize(input.Name);
        var nameAr = Normalize(input.NameAr);
        if (name.Length is < 2 or > 160)
            throw new ArgumentException("Ingredient name must be between 2 and 160 characters.");
        var ingredient = await _db.Ingredients.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (ingredient is null)
            return null;
        if (await _db.Ingredients.AnyAsync(x => x.Id != id && x.Name == name, cancellationToken))
            throw new InvalidOperationException("That ingredient already exists.");

        var oldValue = new { ingredient.Name, ingredient.NameAr, ingredient.IsActive };
        ingredient.Name = name;
        ingredient.NameEn = name;
        ingredient.NameAr = nameAr;
        ingredient.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("ingredient.updated", ingredient.Id, oldValue, new { ingredient.Name, ingredient.NameAr, ingredient.IsActive }, cancellationToken);
        return ToDto(ingredient);
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var ingredient = await _db.Ingredients.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (ingredient is null)
            return false;
        var oldValue = new { ingredient.IsActive };
        ingredient.IsActive = isActive;
        ingredient.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("ingredient.status-changed", ingredient.Id, oldValue, new { ingredient.IsActive }, cancellationToken);
        return true;
    }

    private Task WriteAuditAsync(string action, Guid entityId, object? oldValue, object? newValue, CancellationToken cancellationToken) =>
        _auditLogService?.WriteAsync(action, "Ingredient", entityId, oldValue, newValue, cancellationToken) ?? Task.CompletedTask;

    private Guid RequireTenant() => _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");
    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
    private static IngredientDto ToDto(Ingredient ingredient) => new(ingredient.Id, ingredient.NameEn ?? ingredient.Name, ingredient.IsActive, ingredient.NameAr);
}
