using RestaurantMenuPlatform.Domain.Common;

namespace RestaurantMenuPlatform.Domain.Entities;

public class PriceHistory : TenantEntity
{
    public Guid MenuItemId { get; set; }
    public Guid? BranchId { get; set; }
    public decimal PreviousPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string OperationCode { get; set; } = null!;
    public decimal? ChangeAmount { get; set; }
    public decimal? ChangePercentage { get; set; }
    public string? Reason { get; set; }
    public Guid? ActorUserId { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
    public Branch? Branch { get; set; }
}
