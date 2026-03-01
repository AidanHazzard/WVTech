using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers;

public class HomeController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly ILoginService _loginService;
    private readonly IRegistrationService _registrationService;

    public HomeController(MealPlannerDBContext context, ILoginService loginService, IRegistrationService registrationService)
    {
        _context = context;
        _loginService = loginService;
        _registrationService = registrationService;
    }

    // "/" route:
    // - logged in -> Dashboard
    // - not logged in -> Login page (acts as Landing)
    public IActionResult Index()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        return RedirectToAction("Login", "Login");
    }

    // Authenticated dashboard
    [Authorize]
    public IActionResult Dashboard()
    {
        return View();
    }

    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    // Admin view
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}