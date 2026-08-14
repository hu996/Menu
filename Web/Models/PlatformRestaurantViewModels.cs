using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class PlatformRestaurantIndexViewModel
{
    public IReadOnlyList<PlatformRestaurantDto> Items { get; set; } = [];
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class PlatformRestaurantCreateViewModel
{
    [Required, StringLength(160, MinimumLength = 2)]
    [Display(Name = "English restaurant name")]
    public string NameEn { get; set; } = string.Empty;

    [StringLength(160)]
    [Display(Name = "Arabic restaurant name")]
    public string? NameAr { get; set; }

    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Use lowercase letters, numbers, and hyphens.")]
    [Display(Name = "Restaurant address")]
    public string Slug { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Address { get; set; }

    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "EGP";

    [Required, StringLength(10, MinimumLength = 2)]
    [Display(Name = "Default language")]
    public string DefaultLanguage { get; set; } = "EN";

    [Required]
    [Display(Name = "Plan")]
    public Guid PlanId { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    [Display(Name = "Owner name")]
    public string OwnerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Owner email")]
    public string OwnerEmail { get; set; } = string.Empty;

    [DataType(DataType.Password), StringLength(100, MinimumLength = 10)]
    [Display(Name = "Initial password")]
    public string? OwnerPassword { get; set; }

    public IReadOnlyList<LookupValueDto> Currencies { get; set; } = [];
    public IReadOnlyList<LookupValueDto> Languages { get; set; } = [];
    public IReadOnlyList<PlanDto> Plans { get; set; } = [];
}

public sealed class PlatformRestaurantDetailsViewModel
{
    public PlatformRestaurantDetailsDto Details { get; set; } = null!;
}
