using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record AuthenticatedUserDto(
    Guid UserId,
    Guid TenantId,
    Guid MembershipId,
    string Email,
    string DisplayName,
    string TenantSlug,
    MembershipRole Role,
    Guid? BranchId,
    string SecurityStamp,
    IReadOnlyList<string>? Permissions = null,
    string? TenantName = null);

public sealed record AuthenticationResultDto(
    AuthenticatedUserDto? User,
    string? FailureCode = null)
{
    public bool Succeeded => User is not null;
}
