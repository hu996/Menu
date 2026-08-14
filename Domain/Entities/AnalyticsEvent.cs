using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class AnalyticsEvent : TenantEntity
{
    public AnalyticsEventType EventType { get; set; }
    public Guid BranchId { get; set; }
    public Guid? MenuId { get; set; }
    public Guid? MenuItemId { get; set; }
    public string Device { get; set; } = "unknown";
}
