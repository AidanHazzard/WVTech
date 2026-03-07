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
    private readonly IUserRepository _userRepository;
    private readonly INutritionProgressService? _nutritionProgressService;
    private readonly IRegistrationService _registrationService;

    public FoodEntriesController(
        IRecipeRepository recipeRepository,
        IUserRepository userRepository,
        MealPlannerDBContext context,
        IRegistrationService registrationService,
        INutritionProgressService? nutritionProgressService = null)
    {
        _recipeRepository = recipeRepository;
        _userRepository = userRepository;
        _context = context;
        _registrationService = registrationService;
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

    [Authorize]
    [Route("/FoodEntries/Recipes")]
    public async Task<IActionResult> Recipes()
    {
        User? user = await _registrationService.FindUserByClaimAsync(User);
        IEnumerable<Recipe> userRecipes = [];
        if (user != null)
        {
            userRecipes =  await _userRepository.GetUserOwnedRecipesByUserIdAsync(user.Id);
        }
        return View(userRecipes);
    }

    [HttpGet]
    [Route("/FoodEntries/Recipes/{id}")]
    public async Task<IActionResult> Recipes(int id)
    {
        Recipe? recipe = await _recipeRepository.ReadRecipeWithIngredientsAsync(id);
        
        // Change to not-found view!
        if (recipe == null)
        {
            return RedirectToAction("SearchRecipes"); 
        }

        RecipeViewModel viewModel = ViewModelService.RecipeToRecipeVM(recipe);
        return View("SingleRecipe", viewModel);
    }

    public IActionResult AddNewRecipe()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RecipeAdded(RecipeViewModel newRecipeViewModel)
    {
        //error checking
        if (!ModelState.IsValid)
        {
            return View("AddNewRecipe", newRecipeViewModel);
        }

        Recipe recipe = ViewModelService.RecipeFromRecipeVM(newRecipeViewModel);
        recipe.Owner = await _registrationService.FindUserByClaimAsync(User);

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