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

    public IActionResult SearchRecipes()
    {
        return View();
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
        //error checking
        if (!ModelState.IsValid || newRecipeViewModel.AnyErrors() == true)
        {
            return View("AddNewRecipe", newRecipeViewModel);
        }

        //creates a flattend string of all the entrys from the ingredients list
        string Ingredients = newRecipeViewModel.FlattenList();

        //creates a new recipe model with the viewmodels information
        Recipe recipe = new Recipe();
        recipe.Name = newRecipeViewModel.Name;
        recipe.Ingredients = Ingredients;
        recipe.Directions = newRecipeViewModel.Directions;
        recipe.Meals = new List<Meal>();

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
