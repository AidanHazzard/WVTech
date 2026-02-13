using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers;

public class FoodEntriesController : Controller
{
    public IActionResult SelectType()
    {
        return View();
    }

    public IActionResult Recipes()
    {
        return View();
    }

    public IActionResult AddNewRecipe()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
