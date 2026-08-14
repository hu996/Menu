using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record CartLineInput(Guid MenuItemId, int Quantity, IReadOnlyList<Guid> ModifierOptionIds);

public sealed record CheckoutInput(
    Guid BranchId,
    Guid? MenuId,
    string IdempotencyKey,
    string CustomerName,
    string CustomerPhone,
    string? Notes,
    IReadOnlyList<CartLineInput> Lines,
    Guid? TableId = null,
    Guid? QrCodeId = null,
    string? QrCodeCode = null);
