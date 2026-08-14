namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record RestaurantSettingsDto(
    Guid Id,
    string Name,
    string? NameEn,
    string? NameAr,
    string Slug,
    string? LogoUrl,
    string? CoverImageUrl,
    string? Phone,
    string? Email,
    string? Address,
    string? Currency,
    string DefaultLanguage,
    string? BrandPrimaryColor,
    string? BrandAccentColor,
    bool IsActive,
    IReadOnlyList<LookupValueDto> Currencies,
    IReadOnlyList<LookupValueDto> Languages);

public sealed record RestaurantSettingsInput(
    string NameEn,
    string? NameAr,
    string? Phone,
    string? Email,
    string? Address,
    string Currency,
    string DefaultLanguage,
    string? BrandPrimaryColor,
    string? BrandAccentColor);

public sealed record RestaurantCreationInput(
    string NameEn,
    string? NameAr,
    string Slug,
    string? Phone,
    string? Email,
    string? Address,
    string Currency,
    string DefaultLanguage,
    string? BrandPrimaryColor,
    string? BrandAccentColor);

public sealed record RestaurantCreationResult(
    Guid TenantId,
    Guid MembershipId,
    string TenantSlug,
    string DisplayName,
    string Email,
    string SecurityStamp);

public sealed record TenantBrandingImageDto(
    Guid Id,
    Guid TenantId,
    string Kind,
    string Url,
    string StorageKey,
    string? OriginalFileName,
    string ContentType,
    long SizeBytes,
    string? AltText,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
