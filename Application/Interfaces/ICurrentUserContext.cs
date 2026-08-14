using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? DisplayName { get; }
    Guid? BranchId { get; }
    MembershipRole? Role { get; }
}
