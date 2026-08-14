using System.Security.Claims;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Web.Services;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => Guid.TryParse(
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
        out var id) ? id : null;

    public string? DisplayName => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public Guid? BranchId => Guid.TryParse(
        _httpContextAccessor.HttpContext?.User.FindFirstValue("branch_id"),
        out var id) ? id : null;

    public MembershipRole? Role => Enum.TryParse<MembershipRole>(
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role),
        out var role) ? role : null;
}
