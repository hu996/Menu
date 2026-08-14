using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IOrderService
{
    Task<PublicOrderItemDto?> GetPublicItemAsync(Guid branchId, Guid itemId, string? language = null, CancellationToken cancellationToken = default);
    Task<OrderReceiptDto?> GetPublicOrderAsync(string orderNumber, Guid branchId, CancellationToken cancellationToken = default);
    Task<CartDto?> RecalculateCartAsync(Guid branchId, string restaurantSlug, string branchSlug, IReadOnlyList<CartLineInput> lines, CancellationToken cancellationToken = default, PublicOrderingContextDto? publicContext = null);
    Task<OrderReceiptDto?> CreateAsync(CheckoutInput input, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StaffOrderDto>> GetStaffOrdersAsync(
        Guid? branchScopeId,
        Guid? branchId = null,
        Guid? tableId = null,
        string? status = null,
        string? search = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default);
    Task<StaffOrderDto?> GetStaffOrderAsync(Guid id, Guid? branchId, CancellationToken cancellationToken = default);
    Task<StaffOrderDto?> TransitionAsync(Guid id, OrderStatus targetStatus, Guid? branchId, Guid? actorUserId, string? actorDisplayName, CancellationToken cancellationToken = default);
}
