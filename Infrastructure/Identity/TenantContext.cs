using RestaurantMenuPlatform.Domain.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Identity;

public sealed class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    private bool _isPublic;

    public Guid? TenantId => _tenantId;
    public bool HasTenant => _tenantId.HasValue;
    public bool IsPublic => _isPublic;

    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("A tenant identifier is required.", nameof(tenantId));

        _tenantId = tenantId;
        _isPublic = false;
    }

    public void SetPublicTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("A tenant identifier is required.", nameof(tenantId));

        _tenantId = tenantId;
        _isPublic = true;
    }
}
