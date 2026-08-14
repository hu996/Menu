using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Entities;

public class Order : TenantEntity
{
    public Guid BranchId { get; set; }
    public Guid? TableId { get; set; }
    public Guid? QrCodeId { get; set; }
    public Guid? MenuId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string? Notes { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime? CompletedAtUtc { get; set; }

    public Branch Branch { get; set; } = null!;
    public RestaurantTable? Table { get; set; }
    public QrCode? QrCode { get; set; }
    public Menu? Menu { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
