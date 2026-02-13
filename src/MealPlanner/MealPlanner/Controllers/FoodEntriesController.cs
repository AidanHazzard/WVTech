using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.ViewModels;
using MealPlanner.Models;
using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;

namespace MealPlanner.Controllers;

public class FoodEntriesController : Controller
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly MealPlannerDBContext _context;

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

        return View("Recipes");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
