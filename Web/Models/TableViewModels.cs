using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class TableManagementViewModel
{
    public Guid? BranchId { get; set; }
    public IReadOnlyList<BranchDto> Branches { get; set; } = [];
    public BranchDto? SelectedBranch { get; set; }
    public IReadOnlyList<RestaurantTableDto> Tables { get; set; } = [];
    public TableInputViewModel CreateForm { get; set; } = new();
    public TableInputViewModel? EditForm { get; set; }
    public Guid? EditErrorTableId { get; set; }
}

public sealed class TableInputViewModel
{
    public Guid BranchId { get; set; }
    public Guid? Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? NameAr { get; set; }
}
