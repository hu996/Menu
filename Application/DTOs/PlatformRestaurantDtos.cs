using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record PlatformRestaurantDto(
    Guid Id,
    string Name,
    string? NameAr,
    string Slug,
    string? OwnerName,
    string? OwnerEmail,
    string PlanName,
    SubscriptionStatus Status,
    bool IsActive,
    int Branches,
    int Menus,
    int Products,
    DateTime CreatedAtUtc,
    DateTime? LastActivityAtUtc);

public sealed record PlatformRestaurantDetailsDto(
    PlatformRestaurantDto Restaurant,
    IReadOnlyList<SubscriptionDto> SubscriptionHistory);

public sealed record PlatformRestaurantProvisioningInput(
    string NameEn,
    string? NameAr,
    string Slug,
    string? Phone,
    string? Email,
    string? Address,
    string Currency,
    string DefaultLanguage,
    Guid PlanId,
    string OwnerName,
    string OwnerEmail,
    string? OwnerPassword);

public sealed record PlatformRestaurantProvisioningResult(
    Guid TenantId,
    Guid OwnerUserId,
    Guid MembershipId,
    string TenantSlug,
    string OwnerEmail,
    bool OwnerWasExistingUser);
