namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record BranchInput(
    string Name,
    string? Address,
    string? Phone,
    string? NameAr = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? OpeningHours = null,
    string? BrandPrimaryColorOverride = null,
    string? BrandAccentColorOverride = null);
