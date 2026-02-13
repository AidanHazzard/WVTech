using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.ViewModels;
using MealPlanner.Models;
using MealPlanner.DAL.Abstract;

namespace MealPlanner.Controllers;

public class FoodEntriesController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly IRecipeRepository _recipeRepository;

    public FoodEntriesController(IRecipeRepository recipeRepository, MealPlannerDBContext context)
    {
        _recipeRepository = recipeRepository;
        _context = context;
    }

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
        if (!ModelState.IsValid)
        {
            return View("AddRecipe", newRecipeViewModel);
        }

        string Ingredients = newRecipeViewModel.FlattenList();

        Recipe recipe = new Recipe();
        recipe.Name = newRecipeViewModel.Name;
        recipe.Ingredients = Ingredients;
        recipe.Directions = newRecipeViewModel.Directions;
        recipe.Meals = new List<Meal>();

        _recipeRepository.CreateOrUpdate(recipe);
        _context.SaveChanges();

        var allRecipes = _recipeRepository.ReadAll().ToList();
        Console.WriteLine($"Total recipes: {allRecipes.Count}");
        foreach (var r in allRecipes)
        {
            Console.WriteLine($"Id: {r.Id}, Name: {r.Name}, Ingredients: {r.Ingredients}");
        }

        return View("Recipes");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
