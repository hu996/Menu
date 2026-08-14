using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class ModifierIndexViewModel
{
    public ModifierPageDto Page { get; set; } = new([], 1, 25, 0, null);
}

public sealed class ModifierViewModel
{
    public Guid Id { get; set; }
    [Required, StringLength(160, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(160)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }
    [Display(Name = "Required selection")]
    public bool IsRequired { get; set; }
    [Range(0, 50)]
    public int MinSelections { get; set; }
    [Range(1, 50)]
    public int MaxSelections { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public List<ModifierOptionViewModel> Options { get; set; } = [new()];
}

public sealed class ModifierOptionViewModel
{
    [Required, StringLength(160, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    [StringLength(160)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }
    [Range(-999999999, 999999999)]
    [Display(Name = "Price adjustment")]
    public decimal PriceAdjustment { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
