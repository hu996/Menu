using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class QrCodeManagementViewModel
{
    public Guid? BranchId { get; set; }
    public Guid? TableId { get; set; }
    public IReadOnlyList<BranchDto> Branches { get; set; } = [];
    public BranchDto? SelectedBranch { get; set; }
    public IReadOnlyList<QrCodeDto> Codes { get; set; } = [];
    public IReadOnlyList<RestaurantTableDto> Tables { get; set; } = [];

    [StringLength(4000)]
    [Display(Name = "Table labels")]
    public string? TableLabels { get; set; }
}
