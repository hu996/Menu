using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class BranchViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }

    [StringLength(240)]
    public string? Address { get; set; }

    [Phone, StringLength(40)]
    public string? Phone { get; set; }

    [Range(-90, 90)]
    public decimal? Latitude { get; set; }

    [Range(-180, 180)]
    public decimal? Longitude { get; set; }

    [StringLength(1000)]
    [Display(Name = "Opening hours")]
    public string? OpeningHours { get; set; }

    public string? Slug { get; set; }
    public bool IsActive { get; set; }
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Primary color override")]
    public string? BrandPrimaryColorOverride { get; set; }
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Accent color override")]
    public string? BrandAccentColorOverride { get; set; }
    public bool CanCustomizeBranding { get; set; }
}

public sealed class BranchIndexViewModel
{
    public IReadOnlyList<BranchViewModel> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "name";
    public bool Descending { get; set; }
}
