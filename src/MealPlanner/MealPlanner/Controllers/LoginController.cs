using Microsoft.AspNetCore.Mvc;
using MealPlanner.Services;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers;

public class LoginController : Controller
{
    private readonly ILoginService _loginService;

    public LoginController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpGet("Login")]
    public IActionResult Login() => View();

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _loginService.LoginUserAsync(model);

        if (result.Succeeded) return RedirectToAction("Index", "Home");

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _loginService.LogoutUserAsync();
        return RedirectToAction("Index", "Home");
    }
}