namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record BranchDto(
    Guid Id,
    string Name,
    string Slug,
    string? Address,
    string? Phone,
    bool IsActive,
    string? NameAr = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? OpeningHours = null,
    string? BrandPrimaryColorOverride = null,
    string? BrandAccentColorOverride = null);
