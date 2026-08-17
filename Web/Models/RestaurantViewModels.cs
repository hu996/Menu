using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class RestaurantViewModel
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    [Required, StringLength(160, MinimumLength = 2)]
    [Display(Name = "English name")]
    public string NameEn { get; set; } = string.Empty;
    [StringLength(160)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    [Display(Name = "Logo file")]
    public IFormFile? LogoFile { get; set; }
    [Display(Name = "Cover image file")]
    public IFormFile? CoverFile { get; set; }
    [Phone]
    public string? Phone { get; set; }
    [EmailAddress, StringLength(320)]
    public string? Email { get; set; }
    public string? Address { get; set; }
    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;
    [Required]
    [Display(Name = "Default language")]
    public string DefaultLanguage { get; set; } = string.Empty;
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Primary brand color")]
    public string? BrandPrimaryColor { get; set; }
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Accent brand color")]
    public string? BrandAccentColor { get; set; }
    public IReadOnlyList<LookupValueDto> Currencies { get; set; } = [];
    public IReadOnlyList<LookupValueDto> Languages { get; set; } = [];
}

public sealed class RestaurantCreateViewModel
{
    [Required, StringLength(160, MinimumLength = 2)]
    [Display(Name = "English name")]
    public string NameEn { get; set; } = string.Empty;

    [StringLength(160), Display(Name = "Arabic name")]
    public string? NameAr { get; set; }

    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Use lowercase letters, numbers, and hyphens.")]
    [Display(Name = "Restaurant address")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Logo file")]
    public IFormFile? LogoFile { get; set; }
    [Display(Name = "Cover image file")]
    public IFormFile? CoverFile { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(320)]
    public string? Email { get; set; }

    public string? Address { get; set; }

    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;

    [Required, StringLength(10, MinimumLength = 2)]
    [Display(Name = "Default language")]
    public string DefaultLanguage { get; set; } = string.Empty;

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Primary brand color")]
    public string? BrandPrimaryColor { get; set; }

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a six-digit hex color.")]
    [Display(Name = "Accent brand color")]
    public string? BrandAccentColor { get; set; }

    public IReadOnlyList<LookupValueDto> Currencies { get; set; } = [];
    public IReadOnlyList<LookupValueDto> Languages { get; set; } = [];
}
