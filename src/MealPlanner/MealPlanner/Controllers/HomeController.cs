using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.ViewModels;
using MealPlanner.Models;


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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}