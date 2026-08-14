using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed record PublicProductPageViewModel(
    string RestaurantSlug,
    string BranchSlug,
    PublicOrderItemDto Item,
    CartDto Basket,
    IReadOnlyList<Guid>? SelectedModifierOptionIds = null,
    int InitialQuantity = 1,
    string? EditKey = null);

public sealed record PublicMenuPageViewModel(
    PublicMenuDto Menu,
    CartDto Basket);

public sealed class CheckoutViewModel
{
    public string RestaurantSlug { get; set; } = string.Empty;
    public string BranchSlug { get; set; } = string.Empty;
    public CartDto Cart { get; set; } = new(string.Empty, string.Empty, Guid.Empty, [], 0, string.Empty);

    [Required, StringLength(160)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string CustomerPhone { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required, StringLength(120)]
    public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString("N");
}
