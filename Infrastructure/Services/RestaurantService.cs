using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class RestaurantService : IRestaurantService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ILookupService _lookupService;
    private readonly IEntitlementService _entitlementService;
    private readonly ICurrentUserContext _currentUser;
    private readonly IImageStorage _imageStorage;
    private readonly IAuditLogService? _auditLogService;

    public RestaurantService(
        AppDbContext db,
        ITenantContext tenantContext,
        ILookupService lookupService,
        IEntitlementService entitlementService,
        ICurrentUserContext currentUser,
        IImageStorage imageStorage,
        IAuditLogService? auditLogService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _lookupService = lookupService;
        _entitlementService = entitlementService;
        _currentUser = currentUser;
        _imageStorage = imageStorage;
        _auditLogService = auditLogService;
    }

    public async Task<RestaurantSettingsDto?> GetAsync(CancellationToken cancellationToken = default)
    {
        var tenant = await FindTenantAsync(cancellationToken);
        if (tenant is null)
            return null;
        return await ToDtoAsync(tenant, cancellationToken);
    }

    public async Task<RestaurantCreationResult> CreateAsync(
        RestaurantCreationInput input,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("An authenticated user is required to create a restaurant.");
        var currentTenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("An authenticated tenant context is required.");

        var currentMembership = await _db.Memberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId && x.TenantId == currentTenantId && x.IsActive, cancellationToken);
        if (currentMembership is null || currentMembership.BranchId.HasValue || currentMembership.Role is MembershipRole.BranchManager or MembershipRole.MenuEditor or MembershipRole.Viewer)
            throw new UnauthorizedAccessException("Only a tenant-wide administrator can create a restaurant.");

        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken)
            ?? throw new UnauthorizedAccessException("The authenticated user is no longer active.");
        var nameEn = input.NameEn?.Trim() ?? string.Empty;
        var slug = input.Slug?.Trim().ToLowerInvariant() ?? string.Empty;
        var currency = input.Currency?.Trim().ToUpperInvariant() ?? string.Empty;
        var language = input.DefaultLanguage?.Trim().ToUpperInvariant() ?? string.Empty;

        if (nameEn.Length is < 2 or > 160)
            throw new ArgumentException("English restaurant name must be between 2 and 160 characters.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new ArgumentException("The restaurant address must use lowercase letters, numbers, and hyphens.");
        if (!new EmailAddressAttribute().IsValid(input.Email) && !string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Enter a valid restaurant email address.");
        if (!await _lookupService.IsActiveAsync(LookupTypes.Currency, currency, cancellationToken))
            throw new ArgumentException("Select an active currency from the database lookup catalog.");
        if (!await _lookupService.IsActiveAsync(LookupTypes.Language, language, cancellationToken))
            throw new ArgumentException("Select a supported language from the database lookup catalog.");
        EnsureColor(input.BrandPrimaryColor, "primary brand color");
        EnsureColor(input.BrandAccentColor, "accent brand color");

        if (await _db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug, cancellationToken))
            throw new InvalidOperationException("That restaurant address is already in use.");

        var starterPlan = await _db.Plans
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.MonthlyPrice)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active subscription plan is configured.");
        if ((!string.IsNullOrWhiteSpace(input.BrandPrimaryColor) || !string.IsNullOrWhiteSpace(input.BrandAccentColor)) && !starterPlan.CustomBranding)
            throw new EntitlementViolationException("Custom branding is not included in the starter plan.", FeatureKeys.CustomBranding);

        var tenant = new Tenant
        {
            Name = nameEn,
            NameEn = nameEn,
            NameAr = NullIfEmpty(input.NameAr),
            Slug = slug,
            Phone = NullIfEmpty(input.Phone),
            Email = NullIfEmpty(input.Email),
            Address = NullIfEmpty(input.Address),
            Currency = currency,
            DefaultLanguage = language,
            BrandPrimaryColor = NullIfEmpty(input.BrandPrimaryColor),
            BrandAccentColor = NullIfEmpty(input.BrandAccentColor),
            SubscriptionStatus = SubscriptionStatus.Trial,
            IsActive = true
        };
        var membership = new Membership
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            Role = MembershipRole.TenantOwner,
            IsActive = true
        };
        var subscription = new Subscription
        {
            TenantId = tenant.Id,
            PlanId = starterPlan.Id,
            Status = SubscriptionStatus.Trial,
            StartsAtUtc = DateTime.UtcNow,
            EndsAtUtc = DateTime.UtcNow.AddDays(14)
        };

        await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            // The new tenant is the explicit destination selected by the user.
            // Switching the request-scoped context before SaveChanges keeps the
            // existing tenant ownership guard active for every new row.
            _tenantContext.SetTenant(tenant.Id);
            _db.Tenants.Add(tenant);
            _db.Memberships.Add(membership);
            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync(cancellationToken);
            await DbInitializer.EnsureTenantLookupsAsync(_db, tenant.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        await (_auditLogService?.WriteAsync(
            "restaurant.created",
            "Tenant",
            tenant.Id,
            null,
            new { tenant.NameEn, tenant.NameAr, tenant.Slug, tenant.Currency, tenant.DefaultLanguage, OwnerUserId = user.Id },
            cancellationToken) ?? Task.CompletedTask);

        return new RestaurantCreationResult(tenant.Id, membership.Id, tenant.Slug, user.DisplayName, user.Email, user.SecurityStamp);
    }

    public async Task<RestaurantSettingsDto?> UpdateAsync(
        RestaurantSettingsInput input,
        CancellationToken cancellationToken = default)
    {
        var tenant = await FindTenantAsync(cancellationToken);
        if (tenant is null)
            return null;

        var nameEn = input.NameEn?.Trim() ?? string.Empty;
        var currency = input.Currency?.Trim().ToUpperInvariant() ?? string.Empty;
        var language = input.DefaultLanguage?.Trim().ToUpperInvariant() ?? string.Empty;
        if (nameEn.Length is < 2 or > 160)
            throw new ArgumentException("English restaurant name must be between 2 and 160 characters.");
        if (!new EmailAddressAttribute().IsValid(input.Email) && !string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Enter a valid restaurant email address.");
        if (!await _lookupService.IsActiveAsync(LookupTypes.Currency, currency, cancellationToken))
            throw new ArgumentException("Select an active currency from the database lookup catalog.");
        if (!await _lookupService.IsActiveAsync(LookupTypes.Language, language, cancellationToken))
            throw new ArgumentException("Select a supported language from the database lookup catalog.");
        EnsureColor(input.BrandPrimaryColor, "primary brand color");
        EnsureColor(input.BrandAccentColor, "accent brand color");
        if ((!string.IsNullOrWhiteSpace(input.BrandPrimaryColor) || !string.IsNullOrWhiteSpace(input.BrandAccentColor)) &&
            !await _entitlementService.HasFeatureAsync(FeatureKeys.CustomBranding, cancellationToken))
            throw new EntitlementViolationException("Custom branding is not included in the current plan.", FeatureKeys.CustomBranding);

        var oldValue = new { tenant.NameEn, tenant.NameAr, tenant.LogoUrl, tenant.CoverImageUrl, tenant.Phone, tenant.Email, tenant.Address, tenant.Currency, tenant.DefaultLanguage, tenant.BrandPrimaryColor, tenant.BrandAccentColor };
        tenant.Name = nameEn;
        tenant.NameEn = nameEn;
        tenant.NameAr = NullIfEmpty(input.NameAr);
        tenant.Phone = NullIfEmpty(input.Phone);
        tenant.Email = NullIfEmpty(input.Email);
        tenant.Address = NullIfEmpty(input.Address);
        tenant.Currency = currency;
        tenant.DefaultLanguage = language;
        tenant.BrandPrimaryColor = NullIfEmpty(input.BrandPrimaryColor);
        tenant.BrandAccentColor = NullIfEmpty(input.BrandAccentColor);
        tenant.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await (_auditLogService?.WriteAsync("restaurant.updated", "Tenant", tenant.Id, oldValue, new { tenant.NameEn, tenant.NameAr, tenant.LogoUrl, tenant.CoverImageUrl, tenant.Phone, tenant.Email, tenant.Address, tenant.Currency, tenant.DefaultLanguage, tenant.BrandPrimaryColor, tenant.BrandAccentColor }, cancellationToken) ?? Task.CompletedTask);
        return await ToDtoAsync(tenant, cancellationToken);
    }

    public async Task<TenantBrandingImageDto?> UploadBrandingAsync(
        TenantBrandingKind kind,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("A tenant context is required for branding uploads.");
        var tenant = await _db.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        if (tenant is null)
            return null;

        var stored = await _imageStorage.SaveBrandingAsync(tenantId, content, originalFileName, contentType, length, cancellationToken);
        var existing = await _db.TenantBrandingImages.SingleOrDefaultAsync(x => x.Kind == kind, cancellationToken);
        var previousUrl = existing?.Url;
        try
        {
            if (existing is null)
            {
                existing = new TenantBrandingImage
                {
                    TenantId = tenantId,
                    Kind = kind
                };
                _db.TenantBrandingImages.Add(existing);
            }

            existing.Url = stored.Url;
            existing.StorageKey = stored.StoredFileName;
            existing.OriginalFileName = Path.GetFileName(originalFileName);
            existing.ContentType = contentType.Trim().ToLowerInvariant();
            existing.SizeBytes = length;
            existing.AltText = kind == TenantBrandingKind.Logo ? tenant.NameEn ?? tenant.Name : $"{tenant.NameEn ?? tenant.Name} cover";
            existing.UpdatedAtUtc = DateTime.UtcNow;
            if (kind == TenantBrandingKind.Logo)
                tenant.LogoUrl = stored.Url;
            else
                tenant.CoverImageUrl = stored.Url;
            tenant.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(previousUrl) && !string.Equals(previousUrl, stored.Url, StringComparison.OrdinalIgnoreCase))
                await _imageStorage.DeleteBrandingAsync(tenantId, previousUrl, cancellationToken);
            await (_auditLogService?.WriteAsync(
                kind == TenantBrandingKind.Logo ? "restaurant.logo.uploaded" : "restaurant.cover.uploaded",
                "TenantBrandingImage",
                existing.Id,
                null,
                new { existing.Kind, existing.OriginalFileName, existing.ContentType, existing.SizeBytes },
                cancellationToken) ?? Task.CompletedTask);
            return ToBrandingDto(existing);
        }
        catch
        {
            await _imageStorage.DeleteBrandingAsync(tenantId, stored.Url, cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteBrandingAsync(TenantBrandingKind kind, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("A tenant context is required for branding changes.");
        var tenant = await _db.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        var existing = await _db.TenantBrandingImages.SingleOrDefaultAsync(x => x.Kind == kind, cancellationToken);
        if (tenant is null || existing is null)
            return false;
        var oldUrl = existing.Url;
        _db.TenantBrandingImages.Remove(existing);
        if (kind == TenantBrandingKind.Logo)
            tenant.LogoUrl = null;
        else
            tenant.CoverImageUrl = null;
        tenant.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _imageStorage.DeleteBrandingAsync(tenantId, oldUrl, cancellationToken);
        await (_auditLogService?.WriteAsync(
            kind == TenantBrandingKind.Logo ? "restaurant.logo.deleted" : "restaurant.cover.deleted",
            "TenantBrandingImage",
            existing.Id,
            new { existing.Kind, existing.OriginalFileName },
            null,
            cancellationToken) ?? Task.CompletedTask);
        return true;
    }

    private async Task<Domain.Entities.Tenant?> FindTenantAsync(CancellationToken cancellationToken) =>
        _tenantContext.TenantId.HasValue
            ? await _db.Tenants.SingleOrDefaultAsync(x => x.Id == _tenantContext.TenantId.Value, cancellationToken)
            : null;

    private async Task<RestaurantSettingsDto> ToDtoAsync(Domain.Entities.Tenant tenant, CancellationToken cancellationToken) =>
        new(
            tenant.Id,
            tenant.Name,
            tenant.NameEn,
            tenant.NameAr,
            tenant.Slug,
            tenant.LogoUrl,
            tenant.CoverImageUrl,
            tenant.Phone,
            tenant.Email,
            tenant.Address,
            tenant.Currency,
            tenant.DefaultLanguage.ToUpperInvariant(),
            tenant.BrandPrimaryColor,
            tenant.BrandAccentColor,
            tenant.IsActive,
            await _lookupService.GetActiveAsync(LookupTypes.Currency, cancellationToken),
            await _lookupService.GetActiveAsync(LookupTypes.Language, cancellationToken));

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static TenantBrandingImageDto ToBrandingDto(TenantBrandingImage image) =>
        new(image.Id, image.TenantId, image.Kind.ToString(), image.Url, image.StorageKey, image.OriginalFileName, image.ContentType, image.SizeBytes, image.AltText, image.CreatedAtUtc, image.UpdatedAtUtc);

    private static void EnsureColor(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        if (!System.Text.RegularExpressions.Regex.IsMatch(value.Trim(), "^#[0-9A-Fa-f]{6}$"))
            throw new ArgumentException($"Enter a valid six-digit hex value for the {field}.");
    }
}
