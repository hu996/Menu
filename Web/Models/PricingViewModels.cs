using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class PricingViewModel
{
    [Required]
    public string ScopeCode { get; set; } = string.Empty;

    [Required]
    public string OperationCode { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public Guid? BranchId { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Value { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public List<Guid> MenuItemIds { get; set; } = [];
    public IReadOnlyList<MenuItemDto> Items { get; set; } = [];
    public IReadOnlyList<PricingCategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<BranchDto> Branches { get; set; } = [];
    public IReadOnlyList<LookupValueDto> Operations { get; set; } = [];
    public IReadOnlyList<LookupValueDto> Scopes { get; set; } = [];
    public PricingPreviewDto? Preview { get; set; }
    public IReadOnlyList<PriceHistoryDto> History { get; set; } = [];
}
