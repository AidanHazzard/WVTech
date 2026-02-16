using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.Services;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers
{
    [Authorize(Roles = "User")]
    public class AccountSettingsController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountSettingsController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        

        // GET: /AccountSettings/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new AccountSettingsResetPasswordViewModel());
        }

        // POST: /AccountSettings/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            AccountSettingsResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // logged-in user password reset
            var result = await _accountService.ResetPasswordAsync(
                User,
                model.Password,
                model.NewPassword);

            if (result.Succeeded)
            {
                ViewBag.StatusMessage = "Your password has been reset successfully.";
                ModelState.Clear();
                return View(new AccountSettingsResetPasswordViewModel());
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
    }
}
