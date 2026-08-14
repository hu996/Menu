using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record UserMembershipDto(
    Guid MembershipId,
    Guid UserId,
    string DisplayName,
    string Email,
    MembershipRole Role,
    Guid? BranchId,
    string? BranchName,
    bool IsActive,
    DateTime? LastLoginAtUtc = null,
    int PermissionCount = 0,
    IReadOnlyList<string>? PermissionCodes = null);

public sealed record UserMembershipPageDto(
    IReadOnlyList<UserMembershipDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record UserMembershipInput(
    string DisplayName,
    string Email,
    string Password,
    MembershipRole Role,
    Guid? BranchId,
    IReadOnlyList<string>? PermissionCodes = null);

public sealed record UserMembershipUpdateInput(
    string DisplayName,
    string Email,
    MembershipRole Role,
    Guid? BranchId,
    IReadOnlyList<string>? PermissionCodes = null);

public sealed record PermissionOptionDto(
    string Code,
    string GroupCode,
    string NameEn,
    string NameAr,
    int SortOrder);
