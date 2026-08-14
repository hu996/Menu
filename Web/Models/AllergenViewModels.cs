using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class AllergenIndexViewModel
{
    public AllergenPageDto Page { get; set; } = new([], 1, 25, 0, null);
}

public sealed class AllergenViewModel
{
    public Guid Id { get; set; }
    [Required, StringLength(160, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(160)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }
    public bool IsActive { get; set; }
}
