using Microsoft.AspNetCore.Mvc;
using MealPlanner.Services;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers;

public class AccountController : Controller
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _accountService.LoginUserAsync(model);

        if (result.Succeeded) return RedirectToAction("Index", "Home");

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _accountService.RegisterUserAsync(model);

        if (result.Succeeded) return RedirectToAction("Index", "Home");

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult VerifyEmail() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _accountService.FindUserByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View(model);
        }

        return RedirectToAction("ChangePassword", new { username = user.UserName });
    }

    [HttpGet]
    public IActionResult ChangePassword(string username)
    {
        if (string.IsNullOrEmpty(username)) return RedirectToAction("VerifyEmail");

        return View(new ChangePasswordViewModel { Email = username });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Please correct the errors and try again.");
            return View(model);
        }

        var result = await _accountService.ChangePasswordAsync(model.Email, model.NewPassword);

        if (result.Succeeded) return RedirectToAction("Login");

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutUserAsync();
        return RedirectToAction("Index", "Home");
    }
}
