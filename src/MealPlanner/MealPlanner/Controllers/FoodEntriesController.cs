using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.ViewModels;
using MealPlanner.Models;

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

    [HttpPost]
    public IActionResult RecipeAdded(AddRecipeViewModel newRecipeViewModel)
    {
        string Ingredients = newRecipeViewModel.FlattenList();
        
        Recipe recipe = new Recipe();
        recipe.Name = newRecipeViewModel.Name;
        recipe.Ingredients = Ingredients;
        recipe.Directions = newRecipeViewModel.Directions;
        recipe.Meals = new List<Meal>();
        
        Console.WriteLine(recipe.Directions);
        return View("Recipes");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
