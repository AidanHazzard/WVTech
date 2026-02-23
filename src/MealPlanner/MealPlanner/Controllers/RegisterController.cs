using Microsoft.AspNetCore.Mvc;
using MealPlanner.Services;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers;

public class RegisterController : Controller
{
    private readonly IRegistrationService _registrationService;

    public RegisterController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpGet("Register")]
    public IActionResult Register() => View();

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _registrationService.RegisterUserAsync(model);

        if (result.Succeeded) return RedirectToAction("Index", "Home");

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [HttpGet("VerifyEmail")]
    public IActionResult VerifyEmail() => View();

    [HttpPost("VerifyEmail")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _registrationService.FindUserByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View(model);
        }

        return RedirectToAction("ChangePassword", new { username = user.UserName });
    }

    [HttpGet("ChangePassword")]
    public IActionResult ChangePassword(string username)
    {
        if (string.IsNullOrEmpty(username)) return RedirectToAction("VerifyEmail");

        return View(new ChangePasswordViewModel { Email = username });
    }

    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Please correct the errors and try again.");
            return View(model);
        }

        var result = await _registrationService.ChangePasswordAsync(model.Email, model.NewPassword);

        if (result.Succeeded) return RedirectToAction("Login", "Login");

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    
}
