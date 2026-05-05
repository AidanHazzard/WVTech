using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.ViewModels;
using MealPlanner.Models;
using MealPlanner.DAL.Abstract;
using MealPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Controllers;

public class FoodEntriesController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly IRecipeRepository _recipeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRecipeRepository _userRecipeRepository;
    private readonly INutritionProgressService? _nutritionProgressService;
    private readonly IRegistrationService _registrationService;
    private readonly IExternalRecipeService? _externalRecipeService;

    public FoodEntriesController(
        IRecipeRepository recipeRepository,
        ITagRepository tagRepository,
        IUserRecipeRepository userRecipeRepository,
        MealPlannerDBContext context,
        IRegistrationService registrationService,
        IExternalRecipeService? externalRecipeService = null,
        INutritionProgressService? nutritionProgressService = null)
    {
        _recipeRepository = recipeRepository;
        _tagRepository = tagRepository;
        _context = context;
        _registrationService = registrationService;
        _nutritionProgressService = nutritionProgressService;
        _userRecipeRepository = userRecipeRepository;
        _externalRecipeService = externalRecipeService;
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
    public async Task<IActionResult> NutritionReport(string tab = "weekly")
    {
        if (_nutritionProgressService is null)
            return StatusCode(500, "NutritionProgressService not configured.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var startDay = tab == "monthly" ? today.AddDays(-29) : today.AddDays(-6);

        var progress = await _nutritionProgressService.GetRangeProgressAsync(userId, startDay, today);

        return View(new NutritionReportViewModel { ActiveTab = tab, Progress = progress });
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

    [Route("/FoodEntries/Recipes")]
    public async Task<IActionResult> Recipes()
    {
        User? user = await _registrationService.FindUserByClaimAsync(User);
        IEnumerable<Recipe> userRecipes = [];
        if (user != null)
        {
            userRecipes = await _userRecipeRepository.GetUserOwnedRecipesByUserIdAsync(user.Id);
        }
        IEnumerable<RecipeViewModel> userRecipeVMs = userRecipes.Select(ViewModelService.RecipeToRecipeVM);
        foreach (RecipeViewModel vm in userRecipeVMs)
        {
            if (vm.Id == null) continue;
            vm.VotePercentage = await _userRecipeRepository.GetRecipeVotePercentage(vm.Id ?? 0);
        }
        return View(userRecipeVMs);
    }

    [HttpGet]
    [Route("/FoodEntries/Recipes/{id}")]
    public async Task<IActionResult> Recipes(int id)
    {
        Recipe? recipe = await _recipeRepository.ReadRecipeWithIngredientsAsync(id);
        
        // Get info for external recipe
        if (!recipe?.ExternalUri.IsNullOrEmpty() ?? false && _externalRecipeService != null)
        {
            try
            {
                recipe = await _externalRecipeService.GetExternalRecipeByURI(recipe.ExternalUri!);
                recipe.Id = id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        // Change to not-found view!
        if (recipe == null)
        {
            return RedirectToAction("SearchRecipes");
        }
        RecipeViewModel viewModel = ViewModelService.RecipeToRecipeVM(recipe);
        viewModel.VotePercentage = await _userRecipeRepository.GetRecipeVotePercentage(id);
        
        //if you do not own the recipe
        User? user = await _registrationService.FindUserByClaimAsync(User);
        if (user != null)
        {
            viewModel.UserVote = await _userRecipeRepository.GetUserRecipeVoteAsync(user.Id, id);
            var ownedRecipes = await _userRecipeRepository.GetUserOwnedRecipesByUserIdAsync(user.Id);
            viewModel.IsOwned = ownedRecipes.Any(r => r.Id == id);
        }
        
        return View("SingleRecipe", viewModel);
    }

    public async Task<IActionResult> AddNewRecipe()
    {
        return View(new RecipeViewModel { AvailableTags = await _tagRepository.GetTagNamesAsync() });
    }

    [HttpPost]
    public async Task<IActionResult> RecipeAdded(RecipeViewModel newRecipeViewModel)
    {
        //error checking
        if (!ModelState.IsValid)
        {
            newRecipeViewModel.AvailableTags = await _tagRepository.GetTagNamesAsync();
            return View("AddNewRecipe", newRecipeViewModel);
        }

        Recipe recipe = ViewModelService.RecipeFromRecipeVM(newRecipeViewModel);
        _recipeRepository.CreateOrUpdate(recipe);

        User? user = await _registrationService.FindUserByClaimAsync(User);
        if (user != null)
        {
            UserRecipe userRecipe = new UserRecipe { User = user, Recipe = recipe, UserOwner = true, UserVote = UserVoteType.UpVote };
            _userRecipeRepository.CreateOrUpdate(userRecipe);
        }

        _context.SaveChanges();

        return RedirectToAction("Recipes");
    }

    [HttpGet]
    [Route("/FoodEntries/EditRecipe/{id}")]
    public async Task<IActionResult> EditRecipe(int id)
    {
        Recipe? recipe = await _recipeRepository.ReadRecipeWithIngredientsAsync(id);

        // Change to not-found view!
        if (recipe == null)
        {
            return RedirectToAction("Recipes");
        }

        //if you do not own the recipe redirect
        User? user = await _registrationService.FindUserByClaimAsync(User);
        if (user != null)
        {
            var ownedRecipes = await _userRecipeRepository.GetUserOwnedRecipesByUserIdAsync(user.Id);
            if(ownedRecipes.Any(r => r.Id == id) == false)
            {
                return RedirectToAction("Recipes");
            }
        }

        RecipeViewModel viewModel = ViewModelService.RecipeToRecipeVM(recipe);
        viewModel.AvailableTags = await _tagRepository.GetTagNamesAsync();
        return View("EditRecipe", viewModel);
    }

    [HttpPost]
    [Route("/FoodEntries/EditRecipe/{id}")]
    public async Task<IActionResult> RecipeEditFinished(RecipeViewModel editedRecipeViewModel, int id)
    {
        if (!ModelState.IsValid)
        {
            editedRecipeViewModel.AvailableTags = await _tagRepository.GetTagNamesAsync();
            return View("EditRecipe", editedRecipeViewModel);
        }

        //if you do not own the recipe redirect
        User? user = await _registrationService.FindUserByClaimAsync(User);
        if (user != null)
        {
            var ownedRecipes = await _userRecipeRepository.GetUserOwnedRecipesByUserIdAsync(user.Id);
            if(ownedRecipes.Any(r => r.Id == id) == false)
            {
                return RedirectToAction("Recipes");
            }
        }

        //gets the existing recipe by id
        Recipe? existing = await _recipeRepository.ReadRecipeWithIngredientsAsync(id);
        if (existing == null)
        {
            return RedirectToAction("SearchRecipes");
        }

        //updates the databse with the new and improved existing
        _recipeRepository.CreateOrUpdate(ViewModelService.EditRecipeVMToModel(existing, editedRecipeViewModel));
        _context.SaveChanges();

        return RedirectToAction("Recipes");
    }

  [HttpPost]
[Authorize]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> DeleteRecipe(int id)
{
    User? user = await _registrationService.FindUserByClaimAsync(User);
    if (user == null) return Challenge();

    var ownedRecipes = await _userRecipeRepository.GetUserOwnedRecipesByUserIdAsync(user.Id);
    if (!ownedRecipes.Any(r => r.Id == id))
        return Forbid();

    var recipe = await _recipeRepository.ReadRecipeWithIngredientsAsync(id);
    if (recipe == null) return NotFound();

    // Remove ingredients first
    _context.Set<Ingredient>().RemoveRange(recipe.Ingredients);

    // Remove user recipes
    var userRecipes = _context.Set<UserRecipe>().Where(ur => ur.RecipeId == id);
    _context.Set<UserRecipe>().RemoveRange(userRecipes);

    _context.Recipes.Remove(recipe);
    await _context.SaveChangesAsync();

    return Ok();
}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}