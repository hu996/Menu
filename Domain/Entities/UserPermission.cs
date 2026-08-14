using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class UserPermission : TenantEntity
{
    public Guid MembershipId { get; set; }
    public string PermissionCode { get; set; } = null!;

    public Membership Membership { get; set; } = null!;
}
