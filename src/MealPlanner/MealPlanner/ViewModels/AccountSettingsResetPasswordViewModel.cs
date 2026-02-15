using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels;

public class AccountSettingsResetPasswordViewModel
{
    [Required(ErrorMessage = "Current Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "New Password is required")]
    [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    [Compare("NewPassword", ErrorMessage = "The password does not match.")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
}
