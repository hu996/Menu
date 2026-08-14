using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class ModifierService : IModifierService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogService? _auditLogService;

    public ModifierService(AppDbContext db, ITenantContext tenantContext, IAuditLogService? auditLogService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _auditLogService = auditLogService;
    }

    public async Task<ModifierPageDto> GetPageAsync(string? search, bool? isActive = null, string sortBy = "name", bool descending = false, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 10, 100);
        var query = _db.Modifiers.AsNoTracking().Include(x => x.Options).AsQueryable();
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
        var rows = await ordered
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);
        return new ModifierPageDto(rows.Select(ToDto).ToList(), safePage, safePageSize, total, normalizedSearch);
    }

    public async Task<IReadOnlyList<ModifierDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.Modifiers.AsNoTracking().Include(x => x.Options)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
        return rows.Select(ToDto).ToList();
    }

    public async Task<ModifierDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var modifier = await _db.Modifiers.AsNoTracking().Include(x => x.Options).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return modifier is null ? null : ToDto(modifier);
    }

    public async Task<ModifierDto> CreateAsync(ModifierInput input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        var name = input.Name.Trim();
        if (await _db.Modifiers.AnyAsync(x => x.Name == name, cancellationToken))
            throw new InvalidOperationException("That modifier already exists.");

        var modifier = new Modifier
        {
            TenantId = RequireTenant(),
            Name = name,
            NameEn = name,
            NameAr = Normalize(input.NameAr),
            IsRequired = input.IsRequired,
            MinSelections = input.MinSelections,
            MaxSelections = input.MaxSelections,
            IsActive = input.IsActive
        };
        AddOptions(modifier, input.Options);
        _db.Modifiers.Add(modifier);
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("modifier.created", modifier.Id, null, new { modifier.Name, modifier.IsRequired, modifier.MinSelections, modifier.MaxSelections }, cancellationToken);
        return ToDto(modifier);
    }

    public async Task<ModifierDto?> UpdateAsync(Guid id, ModifierInput input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        var modifier = await _db.Modifiers.Include(x => x.Options).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (modifier is null)
            return null;
        var name = input.Name.Trim();
        if (await _db.Modifiers.AnyAsync(x => x.Id != id && x.Name == name, cancellationToken))
            throw new InvalidOperationException("That modifier already exists.");

        var oldValue = new { modifier.Name, modifier.IsRequired, modifier.MinSelections, modifier.MaxSelections, modifier.IsActive };
        modifier.Name = name;
        modifier.NameEn = name;
        modifier.NameAr = Normalize(input.NameAr);
        modifier.IsRequired = input.IsRequired;
        modifier.MinSelections = input.MinSelections;
        modifier.MaxSelections = input.MaxSelections;
        modifier.IsActive = input.IsActive;
        _db.ModifierOptions.RemoveRange(modifier.Options);
        modifier.Options.Clear();
        AddOptions(modifier, input.Options);
        modifier.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("modifier.updated", modifier.Id, oldValue, new { modifier.Name, modifier.IsRequired, modifier.MinSelections, modifier.MaxSelections, modifier.IsActive }, cancellationToken);
        return ToDto(modifier);
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var modifier = await _db.Modifiers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (modifier is null)
            return false;
        var oldValue = new { modifier.IsActive };
        modifier.IsActive = isActive;
        modifier.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("modifier.status-changed", modifier.Id, oldValue, new { modifier.IsActive }, cancellationToken);
        return true;
    }

    private void AddOptions(Modifier modifier, IReadOnlyList<ModifierOptionInput> options)
    {
        foreach (var option in options.Select((value, index) => (value, index)))
        {
            modifier.Options.Add(new ModifierOption
            {
                TenantId = RequireTenant(),
                Name = option.value.Name.Trim(),
                NameEn = option.value.Name.Trim(),
                NameAr = Normalize(option.value.NameAr),
                PriceAdjustment = option.value.PriceAdjustment,
                SortOrder = option.value.SortOrder == 0 ? option.index + 1 : option.value.SortOrder,
                IsActive = option.value.IsActive
            });
        }
    }

    private static void Validate(ModifierInput input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length is < 2 or > 160)
            throw new ArgumentException("Modifier name must be between 2 and 160 characters.");
        if (input.MinSelections < 0 || input.MaxSelections < 1 || input.MinSelections > input.MaxSelections)
            throw new ArgumentException("Selection limits are invalid.");
        if (input.Options.Count == 0)
            throw new ArgumentException("Add at least one modifier option.");
        if (input.Options.Any(x => string.IsNullOrWhiteSpace(x.Name) || x.Name.Trim().Length > 160))
            throw new ArgumentException("Modifier options require names of 160 characters or fewer.");
        if (input.Options.Any(x => x.PriceAdjustment is < -999999999 or > 999999999))
            throw new ArgumentException("Modifier option prices are outside the allowed range.");
    }

    private Task WriteAuditAsync(string action, Guid entityId, object? oldValue, object? newValue, CancellationToken cancellationToken) =>
        _auditLogService?.WriteAsync(action, "Modifier", entityId, oldValue, newValue, cancellationToken) ?? Task.CompletedTask;

    private Guid RequireTenant() => _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

    private static ModifierDto ToDto(Modifier modifier) => new(
        modifier.Id,
        modifier.NameEn ?? modifier.Name,
        modifier.IsRequired,
        modifier.MinSelections,
        modifier.MaxSelections,
        modifier.IsActive,
        modifier.Options.OrderBy(x => x.SortOrder).Select(x => new ModifierOptionDto(x.Id, x.NameEn ?? x.Name, x.PriceAdjustment, x.SortOrder, x.IsActive, x.NameAr)).ToList(),
        modifier.NameAr);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
