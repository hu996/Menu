using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class BranchService : IBranchService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IEntitlementService _entitlementService;
    private readonly IAuditLogService? _auditLogService;

    public BranchService(
        AppDbContext db,
        ITenantContext tenantContext,
        IEntitlementService entitlementService,
        IAuditLogService? auditLogService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _entitlementService = entitlementService;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(
        Guid? restrictedBranchId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Branches.AsNoTracking().OrderBy(x => x.Name);
        if (restrictedBranchId.HasValue)
            query = (IOrderedQueryable<Branch>)query.Where(x => x.Id == restrictedBranchId.Value);

        return await query
            .Select(x => new BranchDto(x.Id, x.Name, x.Slug, x.Address, x.Phone, x.IsActive, x.NameAr, x.Latitude, x.Longitude, x.OpeningHours, x.BrandPrimaryColorOverride, x.BrandAccentColorOverride))
            .ToListAsync(cancellationToken);
    }

    public async Task<BranchPageDto> GetPageAsync(BranchQuery query, Guid? restrictedBranchId = null, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Clamp(query.PageSize, 5, 50);
        var page = Math.Max(query.Page, 1);
        var branches = _db.Branches.AsNoTracking();
        if (restrictedBranchId.HasValue) branches = branches.Where(x => x.Id == restrictedBranchId.Value);
        if (query.IsActive.HasValue) branches = branches.Where(x => x.IsActive == query.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            branches = branches.Where(x => x.Name.Contains(search) || (x.NameAr != null && x.NameAr.Contains(search)) || x.Slug.Contains(search) || (x.Address != null && x.Address.Contains(search)));
        }
        var total = await branches.CountAsync(cancellationToken);
        var sortBy = query.SortBy?.Trim().ToLowerInvariant() switch { "status" => "status", "address" => "address", _ => "name" };
        var ordered = sortBy switch
        {
            "status" => query.Descending ? branches.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name) : branches.OrderBy(x => x.IsActive).ThenBy(x => x.Name),
            "address" => query.Descending ? branches.OrderByDescending(x => x.Address).ThenBy(x => x.Name) : branches.OrderBy(x => x.Address).ThenBy(x => x.Name),
            _ => query.Descending ? branches.OrderByDescending(x => x.Name) : branches.OrderBy(x => x.Name)
        };
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new BranchDto(x.Id, x.Name, x.Slug, x.Address, x.Phone, x.IsActive, x.NameAr, x.Latitude, x.Longitude, x.OpeningHours, x.BrandPrimaryColorOverride, x.BrandAccentColorOverride))
            .ToListAsync(cancellationToken);
        return new BranchPageDto(items, page, pageSize, total, query.Search, query.IsActive, sortBy, query.Descending);
    }

    public async Task<BranchDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Branches
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BranchDto(x.Id, x.Name, x.Slug, x.Address, x.Phone, x.IsActive, x.NameAr, x.Latitude, x.Longitude, x.OpeningHours, x.BrandPrimaryColorOverride, x.BrandAccentColorOverride))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<BranchDto> CreateAsync(
        BranchInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureValid(input);
        await _entitlementService.EnsureCanCreateBranchAsync(cancellationToken);
        await EnsureBrandingEntitlementAsync(input, cancellationToken);
        var branch = new Branch
        {
            TenantId = RequireTenant(),
            Name = input.Name.Trim(),
            NameEn = input.Name.Trim(),
            NameAr = NullIfEmpty(input.NameAr),
            Slug = await CreateUniqueSlugAsync(input.Name, cancellationToken),
            Address = NullIfEmpty(input.Address),
            Phone = NullIfEmpty(input.Phone),
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            OpeningHours = NullIfEmpty(input.OpeningHours),
            BrandPrimaryColorOverride = NormalizeColor(input.BrandPrimaryColorOverride, "primary branch color"),
            BrandAccentColorOverride = NormalizeColor(input.BrandAccentColorOverride, "accent branch color"),
            IsActive = true
        };

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("branch.created", branch.Id, null, new { branch.Name, branch.NameAr, branch.Slug, branch.Address, branch.Phone, branch.Latitude, branch.Longitude, branch.OpeningHours }, cancellationToken);
        return ToDto(branch);
    }

    public async Task<BranchDto?> UpdateAsync(
        Guid id,
        BranchInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureValid(input);
        await EnsureBrandingEntitlementAsync(input, cancellationToken);
        var branch = await _db.Branches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (branch is null)
            return null;

        var oldValue = new { branch.Name, branch.NameAr, branch.Address, branch.Phone, branch.Latitude, branch.Longitude, branch.OpeningHours, branch.BrandPrimaryColorOverride, branch.BrandAccentColorOverride, branch.IsActive };
        branch.Name = input.Name.Trim();
        branch.NameEn = input.Name.Trim();
        branch.NameAr = NullIfEmpty(input.NameAr);
        branch.Address = NullIfEmpty(input.Address);
        branch.Phone = NullIfEmpty(input.Phone);
        branch.Latitude = input.Latitude;
        branch.Longitude = input.Longitude;
        branch.OpeningHours = NullIfEmpty(input.OpeningHours);
        branch.BrandPrimaryColorOverride = NormalizeColor(input.BrandPrimaryColorOverride, "primary branch color");
        branch.BrandAccentColorOverride = NormalizeColor(input.BrandAccentColorOverride, "accent branch color");
        branch.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("branch.updated", branch.Id, oldValue, new { branch.Name, branch.NameAr, branch.Address, branch.Phone, branch.Latitude, branch.Longitude, branch.OpeningHours, branch.IsActive }, cancellationToken);
        return ToDto(branch);
    }

    public async Task<bool> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (branch is null)
            return false;

        var oldValue = new { branch.IsActive };
        branch.IsActive = isActive;
        branch.UpdatedAtUtc = DateTime.UtcNow;
        if (!isActive)
        {
            var activeQrCodes = await _db.QrCodes
                .Where(x => x.BranchId == id && x.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var qrCode in activeQrCodes)
            {
                qrCode.IsActive = false;
                qrCode.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("branch.status-changed", branch.Id, oldValue, new { branch.IsActive }, cancellationToken);
        return true;
    }

    private Task WriteAuditAsync(
        string action,
        Guid entityId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken) =>
        _auditLogService?.WriteAsync(action, "Branch", entityId, oldValue, newValue, cancellationToken)
        ?? Task.CompletedTask;

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private async Task<string> CreateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = Regex.Replace(name.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "branch";

        var slug = baseSlug;
        var suffix = 2;
        while (await _db.Branches.AnyAsync(x => x.Slug == slug, cancellationToken))
            slug = $"{baseSlug}-{suffix++}";
        return slug;
    }

    private static void EnsureValid(BranchInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length < 2)
            throw new ArgumentException("Branch name is required.");
    }

    private async Task EnsureBrandingEntitlementAsync(BranchInput input, CancellationToken cancellationToken)
    {
        if ((!string.IsNullOrWhiteSpace(input.BrandPrimaryColorOverride) || !string.IsNullOrWhiteSpace(input.BrandAccentColorOverride)) &&
            !await _entitlementService.HasFeatureAsync(FeatureKeys.CustomBranding, cancellationToken))
            throw new EntitlementViolationException("Custom branding is not included in the current plan.", FeatureKeys.CustomBranding);
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeColor(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var color = value.Trim();
        if (!Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$")) throw new ArgumentException($"Enter a valid six-digit hex value for the {field}.");
        return color.ToUpperInvariant();
    }

    private static BranchDto ToDto(Branch branch) =>
        new(branch.Id, branch.Name, branch.Slug, branch.Address, branch.Phone, branch.IsActive, branch.NameAr, branch.Latitude, branch.Longitude, branch.OpeningHours, branch.BrandPrimaryColorOverride, branch.BrandAccentColorOverride);
}
