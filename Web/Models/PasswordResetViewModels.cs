using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class ForgotPasswordViewModel
{
    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordViewModel
{
    [Required, StringLength(128)]
    public string Token { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10), DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, StringLength(128), Compare(nameof(NewPassword)), DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed record ForgotPasswordConfirmationViewModel(string? DevelopmentResetUrl);
