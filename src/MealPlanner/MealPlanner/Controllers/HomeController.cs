using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlanner.ViewModels;
using MealPlanner.Models;
using Microsoft.AspNetCore.Authorization;
using MealPlanner.Services;
using System.Security.Claims;

namespace MealPlanner.Controllers;

public class HomeController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly IAccountService _accountService;

    public HomeController(MealPlannerDBContext context, IAccountService accountService)
    {
        _context = context;
        _accountService = accountService;
    }

    public async Task<IActionResult> Index()
    {

        return View();
    }

    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    //This is the specific admin view
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }
    // This is the specific user view or dashboard
    [Authorize(Roles = "User")]
    public IActionResult User()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
