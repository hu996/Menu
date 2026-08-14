using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class MenuViewModel
{
    public Guid Id { get; set; }
    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(120)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }
    public string? Slug { get; set; }
    public bool IsGlobal { get; set; }
    [Display(Name = "Menu type")]
    public string? MenuTypeCode { get; set; }
    [Display(Name = "Menu scope")]
    public string? ScopeCode { get; set; }
    [StringLength(500)] public string? Description { get; set; }
    [StringLength(500), Display(Name = "Arabic description")] public string? DescriptionAr { get; set; }
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Primary display color")] public string? BrandPrimaryColor { get; set; }
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Accent display color")] public string? BrandAccentColor { get; set; }
    [Range(0, 10000), Display(Name = "Display order")] public int SortOrder { get; set; }
    public IReadOnlyList<LookupValueDto> MenuTypes { get; set; } = [];
    public IReadOnlyList<LookupValueDto> MenuScopes { get; set; } = [];
    public List<Guid> BranchIds { get; set; } = [];
    public IReadOnlyList<BranchDto> Branches { get; set; } = [];
    public bool CanCustomizeBranding { get; set; }
}

public sealed class MenuCategoryViewModel
{
    public Guid MenuId { get; set; }
    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(120)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }
    [StringLength(500)]
    public string? Description { get; set; }
    [StringLength(500)]
    [Display(Name = "Arabic description")]
    public string? DescriptionAr { get; set; }
    [Display(Name = "Classification")]
    public string? ClassificationCode { get; set; }
    public IReadOnlyList<LookupValueDto> Classifications { get; set; } = [];
    public IReadOnlyList<MenuCategoryDto> ParentCategories { get; set; } = [];
    public Guid? ParentCategoryId { get; set; }
    [Range(0, 10000)]
    public int SortOrder { get; set; }
}

public sealed class MenuItemViewModel
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    [Required, StringLength(160, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(160)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }
    [StringLength(1000)]
    public string? Description { get; set; }
    [StringLength(1000)]
    [Display(Name = "Arabic description")]
    public string? DescriptionAr { get; set; }
    [Range(typeof(decimal), "0", "999999999")]
    public decimal Price { get; set; }
    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;
    [Range(0, 10000)]
    public int SortOrder { get; set; }
    public List<Guid> IngredientIds { get; set; } = [];
    public List<Guid> AllergenIds { get; set; } = [];
    [Display(Name = "Product type")]
    public string? ProductTypeCode { get; set; }
    public List<Guid> ModifierIds { get; set; } = [];
    public bool IsAvailable { get; set; }
    public string? CategoryName { get; set; }
    public string? MenuName { get; set; }
    public IReadOnlyList<ProductBranchAvailabilityDto> BranchAvailability { get; set; } = [];
    public IReadOnlyList<IngredientDto> IngredientOptions { get; set; } = [];
    public IReadOnlyList<AllergenDto> AllergenOptions { get; set; } = [];
    public IReadOnlyList<MenuCategoryDto> CategoryOptions { get; set; } = [];
    public IReadOnlyList<RestaurantMenuPlatform.Application.DTOs.MenuItemImageDto> Images { get; set; } = [];
    public IReadOnlyList<RestaurantMenuPlatform.Application.DTOs.LookupValueDto> CurrencyOptions { get; set; } = [];
    public IReadOnlyList<LookupValueDto> ProductTypes { get; set; } = [];
    public IReadOnlyList<ModifierDto> Modifiers { get; set; } = [];
}
