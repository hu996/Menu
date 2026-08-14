namespace RestaurantMenuPlatform.Domain.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    bool HasTenant { get; }
    bool IsPublic { get; }

    void SetTenant(Guid tenantId);
    void SetPublicTenant(Guid tenantId);
}
