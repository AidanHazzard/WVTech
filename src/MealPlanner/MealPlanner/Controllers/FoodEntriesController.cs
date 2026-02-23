using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.ViewModels;
using MealPlanner.Models;
using MealPlanner.DAL.Abstract;
using MealPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MealPlanner.Controllers;

public class FoodEntriesController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly IRecipeRepository _recipeRepository;
    private readonly INutritionProgressService? _nutritionProgressService;

    public FoodEntriesController(
        IRecipeRepository recipeRepository,
        MealPlannerDBContext context,
        INutritionProgressService? nutritionProgressService = null)
    {
        _recipeRepository = recipeRepository;
        _context = context;
        _nutritionProgressService = nutritionProgressService;
    }   

    public IActionResult SearchRecipes()
    {
        return View();
    }

    public IActionResult SelectType()
    {
        return View();
    }

    [Route("/FoodEntries/Recipes")]
    public IActionResult Recipes()
    {
        return View();
    }

    [HttpGet]
    [Route("/FoodEntries/Recipes/{id}")]
    public async Task<IActionResult> Recipes(int id)
    {
        Recipe? recipe = await _recipeRepository.ReadRecipeWithIngredientsAsync(id);
        // Change to not-found error!
        if (recipe == null) { return RedirectToAction("SelectType"); }
        RecipeViewModel viewModel = RecipeViewModel.FromRecipe(recipe);
        return View("SingleRecipe", viewModel);
    }

    public IActionResult AddNewRecipe()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Nutrition()
    {
        if (_nutritionProgressService is null)
            return StatusCode(500, "NutritionProgressService not configured.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var progress = await _nutritionProgressService.GetDailyProgressAsync(userId, today);

        return View(progress);
    }

    [HttpPost]
    public IActionResult RecipeAdded(RecipeViewModel newRecipeViewModel)
    {
        //error checking
        if (!ModelState.IsValid || newRecipeViewModel.AnyErrors() == true)
        {
            return View("AddNewRecipe", newRecipeViewModel);
        }

        //creates a new recipe model with the viewmodels information
        Recipe recipe = new Recipe();
        recipe.Name = newRecipeViewModel.Name;
        recipe.Ingredients = [];
        recipe.Directions = newRecipeViewModel.Directions;
        recipe.Calories = newRecipeViewModel.Calories;
        recipe.Protein = newRecipeViewModel.Protein;
        recipe.Carbs = newRecipeViewModel.Carbs;
        recipe.Fat = newRecipeViewModel.Fat;
        recipe.Meals = new List<Meal>();

        // Adds the ingredients to the recipe
        foreach (string i in newRecipeViewModel.Ingredients)
        {
            Ingredient newIngredient = new Ingredient
            {
                IngredientBase = new IngredientBase { Name = i },
                Measurement = new Measurement { Name = "count" },
                Amount = 1
            };

            recipe.Ingredients.Add(newIngredient);
        }

        //adds it to the database
        _recipeRepository.CreateOrUpdate(recipe);
        _context.SaveChanges();

        return View("Recipes");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
