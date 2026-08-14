using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class PlanManagementIndexViewModel
{
    public IReadOnlyList<PlanDto> Plans { get; set; } = [];
}

public sealed class PlanFormViewModel
{
    public Guid? Id { get; set; }

    [Required, StringLength(160, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal MonthlyPrice { get; set; }

    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;

    public IReadOnlyList<LookupValueDto> CurrencyOptions { get; set; } = [];

    [Range(0, int.MaxValue)]
    public int MaxBranches { get; set; }

    [Range(0, int.MaxValue)]
    public int MaxMenuItems { get; set; }

    [Range(0, int.MaxValue)]
    public int MaxUsers { get; set; }

    public bool AdvancedAnalytics { get; set; }
    public bool CustomBranding { get; set; }
    public bool IsActive { get; set; } = true;

    [Display(Name = "Feature definitions")]
    public string FeaturesText { get; set; } = string.Empty;
}
