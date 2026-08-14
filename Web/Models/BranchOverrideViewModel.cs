using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class BranchOverrideViewModel
{
    public Guid BranchId { get; set; }
    public Guid MenuItemId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal GlobalPrice { get; set; }
    public decimal? PriceOverride { get; set; }
    public bool? IsAvailableOverride { get; set; }
    public bool? IsVisibleOverride { get; set; }
    public decimal EffectivePrice { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsVisible { get; set; }
}

public sealed class BranchOverrideEditViewModel
{
    public Guid BranchId { get; set; }
    public Guid MenuItemId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal GlobalPrice { get; set; }
    [Range(typeof(decimal), "0", "999999999")]
    public decimal? PriceOverride { get; set; }
    public bool? IsAvailableOverride { get; set; }
    public bool? IsVisibleOverride { get; set; }
}
