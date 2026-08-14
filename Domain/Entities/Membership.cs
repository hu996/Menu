using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Membership : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid? BranchId { get; set; }
    public MembershipRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public Branch? Branch { get; set; }
}
