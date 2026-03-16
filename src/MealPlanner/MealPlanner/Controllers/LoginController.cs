using Microsoft.AspNetCore.Mvc;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Controllers;

public class LoginController : Controller
{
    private readonly ILoginService _loginService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(ILoginService loginService, ILogger<LoginController> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }

    [HttpGet("Login")]
    public IActionResult Login() => View();

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        _logger.LogInformation("--------------------Login POST received. Email: {Email}, RememberMe: {RememberMe}---------------------", model.Email, model.RememberMe);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login model invalid.");
            return View(model);
        }

        var result = await _loginService.LoginUserAsync(model);

        _logger.LogInformation("--------------------Login result returned: {Result}---------------------", result.ToString());

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError("", "You must confirm your email before logging in. Please check your inbox.");
            return View(model);
        }

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