using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Identity;

public sealed class MembershipAuthorizationService : IMembershipAuthorizationService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public MembershipAuthorizationService(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<bool> CanAccessBranchAsync(
        Guid userId,
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
            return false;

        var tenantId = _tenantContext.TenantId!.Value;

        var membership = await _db.Memberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.UserId == userId &&
                x.TenantId == tenantId &&
                x.IsActive &&
                x.User.IsActive,
                cancellationToken);

        if (membership is null)
            return false;

        // Validate the branch before evaluating role scope so this service
        // remains an ownership check when called outside an MVC controller.
        if (!await _db.Branches.AnyAsync(x => x.Id == branchId, cancellationToken))
            return false;

        if (membership.Role is MembershipRole.PlatformAdmin or MembershipRole.TenantOwner or MembershipRole.TenantAdmin)
            return true;

        return !membership.BranchId.HasValue || membership.BranchId == branchId;
    }
}
