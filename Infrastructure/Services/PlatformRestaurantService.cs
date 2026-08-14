using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Identity;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class PlatformRestaurantService : IPlatformRestaurantService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly PasswordService _passwordService;
    private readonly ILookupService _lookupService;
    private readonly IAuditLogService _auditLogService;

    public PlatformRestaurantService(
        AppDbContext db,
        ITenantContext tenantContext,
        ICurrentUserContext currentUser,
        PasswordService passwordService,
        ILookupService lookupService,
        IAuditLogService auditLogService)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _passwordService = passwordService;
        _lookupService = lookupService;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyList<PlatformRestaurantDto>> GetAllAsync(
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        EnsurePlatformOwner();

        var tenantsQuery = _db.Tenants
            .IgnoreQueryFilters()
            .Where(x => x.Slug != DbInitializer.PlatformSystemTenantSlug)
            .AsNoTracking()
            .AsQueryable();
        if (isActive.HasValue)
            tenantsQuery = tenantsQuery.Where(x => x.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            tenantsQuery = tenantsQuery.Where(x =>
                x.Name.Contains(term) ||
                (x.NameEn != null && x.NameEn.Contains(term)) ||
                x.Slug.Contains(term) ||
                (x.Email != null && x.Email.Contains(term)));
        }

        var tenants = await tenantsQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        return await BuildDtosAsync(tenants, cancellationToken);
    }

    public async Task<PlatformRestaurantDetailsDto?> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        EnsurePlatformOwner();
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        if (tenant is null)
            return null;

        var dto = (await BuildDtosAsync([tenant], cancellationToken)).Single();
        var subscriptions = await _db.Subscriptions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.StartsAtUtc)
            .Select(x => new SubscriptionDto(
                x.Id,
                x.PlanId,
                x.Plan.Name,
                x.Status,
                x.StartsAtUtc,
                x.EndsAtUtc,
                x.PaymentProvider,
                x.ExternalSubscriptionId))
            .ToListAsync(cancellationToken);
        return new PlatformRestaurantDetailsDto(dto, subscriptions);
    }

    public async Task<IReadOnlyList<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        EnsurePlatformOwner();
        return await _db.Plans
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.IsActive)
            .OrderBy(x => x.MonthlyPrice)
            .ThenBy(x => x.Name)
            .Select(x => new PlanDto(
                x.Id,
                x.Name,
                x.MonthlyPrice,
                x.Currency,
                x.MaxBranches,
                x.MaxMenuItems,
                x.MaxUsers,
                x.AdvancedAnalytics,
                x.CustomBranding,
                x.IsActive,
                x.Features.OrderBy(feature => feature.FeatureKey)
                    .Select(feature => new PlanFeatureDto(feature.FeatureKey, feature.Enabled, feature.LimitValue))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<PlatformRestaurantProvisioningResult> ProvisionAsync(
        PlatformRestaurantProvisioningInput input,
        CancellationToken cancellationToken = default)
    {
        EnsurePlatformOwner();

        var nameEn = input.NameEn?.Trim() ?? string.Empty;
        var nameAr = NullIfEmpty(input.NameAr);
        var slug = input.Slug?.Trim().ToLowerInvariant() ?? string.Empty;
        var ownerName = input.OwnerName?.Trim() ?? string.Empty;
        var ownerEmail = input.OwnerEmail?.Trim() ?? string.Empty;
        var ownerNormalizedEmail = ownerEmail.ToUpperInvariant();
        var currency = input.Currency?.Trim().ToUpperInvariant() ?? string.Empty;
        var language = input.DefaultLanguage?.Trim().ToUpperInvariant() ?? string.Empty;

        if (nameEn.Length is < 2 or > 160)
            throw new ArgumentException("English restaurant name must be between 2 and 160 characters.");
        if (!Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new ArgumentException("The restaurant address must use lowercase letters, numbers, and hyphens.");
        if (!new EmailAddressAttribute().IsValid(ownerEmail))
            throw new ArgumentException("Enter a valid owner email address.");
        if (ownerName.Length is < 2 or > 120)
            throw new ArgumentException("Owner name must be between 2 and 120 characters.");
        if (!string.IsNullOrWhiteSpace(input.Email) && !new EmailAddressAttribute().IsValid(input.Email))
            throw new ArgumentException("Enter a valid restaurant email address.");
        if (!await _lookupService.IsActiveAsync(Domain.Constants.LookupTypes.Currency, currency, cancellationToken))
            throw new ArgumentException("Select an active currency from the database lookup catalog.");
        if (!await _lookupService.IsActiveAsync(Domain.Constants.LookupTypes.Language, language, cancellationToken))
            throw new ArgumentException("Select a supported language from the database lookup catalog.");

        var plan = await _db.Plans
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == input.PlanId && x.IsActive, cancellationToken)
            ?? throw new ArgumentException("Select an active subscription plan.");

        if (await _db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug, cancellationToken))
            throw new InvalidOperationException("That restaurant address is already in use.");

        var existingUser = await _db.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.NormalizedEmail == ownerNormalizedEmail, cancellationToken);
        if (existingUser is null && string.IsNullOrWhiteSpace(input.OwnerPassword))
            throw new ArgumentException("Set an initial owner password or use an existing registered owner email.");
        if (existingUser is null)
            PasswordService.ValidateStrength(input.OwnerPassword!);

        if (existingUser is not null)
        {
            var existingMemberships = await _db.Memberships
                .IgnoreQueryFilters()
                .Where(x => x.UserId == existingUser.Id && x.IsActive)
                .Select(x => x.Role)
                .ToListAsync(cancellationToken);
            if (existingMemberships.Contains(MembershipRole.PlatformAdmin))
                throw new InvalidOperationException("A platform owner account cannot be assigned as a restaurant owner.");
            if (existingMemberships.Count > 0)
                throw new InvalidOperationException("That owner already has an active restaurant membership. Use an owner account with one active restaurant membership.");
        }
        var tenant = new Tenant
        {
            Name = nameEn,
            NameEn = nameEn,
            NameAr = nameAr,
            Slug = slug,
            Phone = NullIfEmpty(input.Phone),
            Email = NullIfEmpty(input.Email),
            Address = NullIfEmpty(input.Address),
            Currency = currency,
            DefaultLanguage = language,
            SubscriptionStatus = SubscriptionStatus.Trial,
            IsActive = true
        };
        var owner = existingUser ?? new User
        {
            Email = ownerEmail,
            NormalizedEmail = ownerNormalizedEmail,
            DisplayName = ownerName,
            PasswordHash = _passwordService.Hash(input.OwnerPassword!)
        };
        if (existingUser is not null)
        {
            owner.DisplayName = ownerName;
            owner.IsActive = true;
        }

        var membership = new Membership
        {
            TenantId = tenant.Id,
            User = owner,
            Role = MembershipRole.TenantOwner,
            IsActive = true
        };
        var subscription = new Subscription
        {
            TenantId = tenant.Id,
            PlanId = plan.Id,
            Status = SubscriptionStatus.Trial,
            StartsAtUtc = DateTime.UtcNow,
            EndsAtUtc = DateTime.UtcNow.AddDays(14)
        };

        await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            _tenantContext.SetTenant(tenant.Id);
            _db.Tenants.Add(tenant);
            if (existingUser is null)
                _db.Users.Add(owner);
            _db.Memberships.Add(membership);
            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync(cancellationToken);
            await DbInitializer.EnsureTenantLookupsAsync(_db, tenant.Id, cancellationToken);
            await _auditLogService.WriteAsync(
                "platform.restaurant-provisioned",
                "Tenant",
                tenant.Id,
                null,
                new { tenant.NameEn, tenant.Slug, OwnerUserId = owner.Id, OwnerEmail = owner.Email, PlanId = plan.Id },
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        return new PlatformRestaurantProvisioningResult(
            tenant.Id,
            owner.Id,
            membership.Id,
            tenant.Slug,
            owner.Email,
            existingUser is not null);
    }

    public async Task<bool> SetActiveAsync(
        Guid tenantId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        EnsurePlatformOwner();
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        if (tenant is null)
            return false;
        if (tenant.IsActive == isActive)
            return true;

        tenant.IsActive = isActive;
        tenant.UpdatedAtUtc = DateTime.UtcNow;
        _tenantContext.SetTenant(tenantId);
        await _db.SaveChangesAsync(cancellationToken);
        await _auditLogService.WriteAsync(
            isActive ? "platform.restaurant-activated" : "platform.restaurant-deactivated",
            "Tenant",
            tenant.Id,
            new { IsActive = !isActive },
            new { tenant.IsActive },
            cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<PlatformRestaurantDto>> BuildDtosAsync(
        IReadOnlyList<Tenant> tenants,
        CancellationToken cancellationToken)
    {
        var tenantIds = tenants.Select(x => x.Id).ToArray();
        var owners = await _db.Memberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => tenantIds.Contains(x.TenantId) && x.Role == MembershipRole.TenantOwner && x.IsActive)
            .ToListAsync(cancellationToken);
        var subscriptions = await _db.Subscriptions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Plan)
            .Where(x => tenantIds.Contains(x.TenantId))
            .OrderByDescending(x => x.StartsAtUtc)
            .ToListAsync(cancellationToken);
        var branches = await _db.Branches.IgnoreQueryFilters().AsNoTracking().Where(x => tenantIds.Contains(x.TenantId)).GroupBy(x => x.TenantId).Select(x => new { TenantId = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.TenantId, x => x.Count, cancellationToken);
        var menus = await _db.Menus.IgnoreQueryFilters().AsNoTracking().Where(x => tenantIds.Contains(x.TenantId)).GroupBy(x => x.TenantId).Select(x => new { TenantId = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.TenantId, x => x.Count, cancellationToken);
        var products = await _db.MenuItems.IgnoreQueryFilters().AsNoTracking().Where(x => tenantIds.Contains(x.TenantId)).GroupBy(x => x.TenantId).Select(x => new { TenantId = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.TenantId, x => x.Count, cancellationToken);
        var activity = await _db.AuditLogs.IgnoreQueryFilters().AsNoTracking().Where(x => tenantIds.Contains(x.TenantId)).GroupBy(x => x.TenantId).Select(x => new { TenantId = x.Key, Last = x.Max(log => (DateTime?)log.CreatedAtUtc) }).ToDictionaryAsync(x => x.TenantId, x => x.Last, cancellationToken);

        return tenants.Select(tenant =>
        {
            var subscription = subscriptions.FirstOrDefault(x => x.TenantId == tenant.Id);
            var owner = owners.FirstOrDefault(x => x.TenantId == tenant.Id);
            return new PlatformRestaurantDto(
                tenant.Id,
                tenant.NameEn ?? tenant.Name,
                tenant.NameAr,
                tenant.Slug,
                owner?.User.DisplayName,
                owner?.User.Email,
                subscription?.Plan.Name ?? "Unassigned",
                subscription?.Status ?? tenant.SubscriptionStatus,
                tenant.IsActive,
                branches.GetValueOrDefault(tenant.Id),
                menus.GetValueOrDefault(tenant.Id),
                products.GetValueOrDefault(tenant.Id),
                tenant.CreatedAtUtc,
                activity.GetValueOrDefault(tenant.Id));
        }).ToList();
    }

    private void EnsurePlatformOwner()
    {
        if (_currentUser.Role != MembershipRole.PlatformAdmin)
            throw new UnauthorizedAccessException("Platform owner access is required.");
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
