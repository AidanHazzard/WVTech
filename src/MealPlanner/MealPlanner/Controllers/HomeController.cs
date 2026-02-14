using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlanner.ViewModels;
using MealPlanner.Models;
using Microsoft.AspNetCore.Authorization;

namespace MealPlanner.Controllers;
public class HomeController : Controller
{
   private readonly MealPlannerDBContext _context;

public HomeController(MealPlannerDBContext context)
{
    _context = context;
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

    public IActionResult Register()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }

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