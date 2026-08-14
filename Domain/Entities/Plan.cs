using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Plan : BaseEntity
{
    public string Name { get; set; } = null!;
    public decimal MonthlyPrice { get; set; }
    public string Currency { get; set; } = null!;
    public int MaxBranches { get; set; }
    public int MaxMenuItems { get; set; }
    public int MaxUsers { get; set; }
    public bool AdvancedAnalytics { get; set; }
    public bool CustomBranding { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();
}
