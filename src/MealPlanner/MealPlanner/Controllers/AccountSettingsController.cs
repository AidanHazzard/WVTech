using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Controllers
{
    [Authorize(Roles = "User")]
    public class AccountSettingsController : Controller
    {
        private readonly IAccountSettingsService _accountSettingsService;

        public AccountSettingsController(
            IAccountSettingsService accountSettingsService)
        {
            _accountSettingsService = accountSettingsService;
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new AccountSettingsResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            AccountSettingsResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountSettingsService.ChangePasswordAsync(
                User,
                model.Password,
                model.NewPassword);

            if (result.Succeeded)
                return ResetPasswordSuccess();

            AddErrors(result);
            return View(model);
        }

        // --- private helpers (controller-only concerns) ---

        private IActionResult ResetPasswordSuccess()
        {
            ViewBag.StatusMessage = "Your password has been reset successfully.";
            ModelState.Clear();
            return View(new AccountSettingsResetPasswordViewModel());
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
