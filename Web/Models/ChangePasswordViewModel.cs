using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class ChangePasswordViewModel
{
    [Required, StringLength(128), DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10), DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, StringLength(128), Compare(nameof(NewPassword)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
