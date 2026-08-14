using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class RolePermission : BaseEntity
{
    public MembershipRole Role { get; set; }
    public string PermissionCode { get; set; } = null!;
}
