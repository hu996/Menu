using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class ProductIndexViewModel
{
    public ProductPageDto Page { get; set; } = new([], 1, 25, 0, null, null, null, null, null, "name", false);
    public ProductFilterOptionsDto Options { get; set; } = new([], [], []);
    public bool CanBulkEdit { get; set; }
}
