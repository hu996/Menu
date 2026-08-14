using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class UserIndexViewModel
{
    public UserMembershipPageDto Page { get; set; } = new([], 1, 25, 0, null);
}

public sealed class UserCreateViewModel
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 10), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public MembershipRole Role { get; set; } = MembershipRole.Viewer;

    public Guid? BranchId { get; set; }
    public IReadOnlyList<BranchDto> Branches { get; set; } = [];
    public List<string> PermissionCodes { get; set; } = [];
    public IReadOnlyList<PermissionOptionDto> PermissionOptions { get; set; } = [];
}

public sealed class UserEditViewModel
{
    public Guid MembershipId { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public MembershipRole Role { get; set; } = MembershipRole.Viewer;

    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; }
    public List<string> PermissionCodes { get; set; } = [];
    public IReadOnlyList<BranchDto> Branches { get; set; } = [];
    public IReadOnlyList<PermissionOptionDto> PermissionOptions { get; set; } = [];
}
