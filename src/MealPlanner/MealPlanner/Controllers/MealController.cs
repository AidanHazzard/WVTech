using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Controllers;

[Authorize]
public class MealController : Controller
{
    private readonly IRegistrationService _registrationService;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IMealRepository _mealRepo;
    private readonly MealPlannerDBContext _context;
    private readonly IMealRecommendationService? _recommendationService;

    public MealController(
        IRegistrationService registrationService,
        IRecipeRepository recipeRepo,
        IMealRepository mealRepo,
        MealPlannerDBContext context,
        IMealRecommendationService? mealRecommendationService = null)
    {
        _registrationService = registrationService;
        _recipeRepo = recipeRepo;
        _mealRepo = mealRepo;
        _context = context;
        _recommendationService = mealRecommendationService;
    }

    public async Task<IActionResult> PlannerHome(string? date)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        DateTime selectedDate =
            DateTime.TryParse(date, out var parsed)
                ? parsed.Date
                : DateTime.Today;

        var meals = await _mealRepo.GetUserMealsByDateAsync(user, selectedDate);

        var vm = new PlannerHomeViewModel
        {
            SelectedDate = selectedDate,
            Meals = meals
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult NewMeal()
    {
        return View(new CreateMealViewModel
        {
            SelectedMonth = DateTime.Today.Month,
            SelectedDay = DateTime.Today.Day
        });
    }

    [HttpPost]
    public async Task<IActionResult> NewMeal(CreateMealViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var year = DateTime.Today.Year;
        DateTime selectedDate;

        try
        {
            selectedDate = new DateTime(year, model.SelectedMonth, model.SelectedDay);
        }
        catch
        {
            ModelState.AddModelError("", "Please select a valid month and day.");
            return View(model);
        }

        Meal newMeal = new Meal
        {
            User = user,
            UserId = user.Id,
            Title = model.Title.Trim(),
            StartTime = selectedDate,
            RepeatRule = model.RepeatWeekly ? "Weekly" : null
        };

        foreach (int id in model.RecipeIds)
        {
            var recipe = _recipeRepo.Read(id);
            if (recipe != null)
            {
                newMeal.Recipes.Add(recipe);
            }
        }

        _mealRepo.CreateOrUpdate(newMeal);
        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> GenerateMeal(CreateMealViewModel model)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();
        if (_recommendationService == null) return Problem(statusCode:500);
        Meal newMeal = new Meal
        {
            User = user,
            Title = model.Title.Trim(),
            StartTime = model.Date.Date
        };
        newMeal.Recipes = await _recommendationService.GetRecommendedRecipesForUser(user, model.Date.Date);
        if (newMeal.Recipes.IsNullOrEmpty()) return NotFound();

        newMeal = _mealRepo.CreateOrUpdate(newMeal);
        _context.SaveChanges();
        return RedirectToAction("ViewMeal", new {id = newMeal.Id });
    }

    [HttpGet]
    public async Task<IActionResult> ViewMeal(int id)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var meal = await _mealRepo.ReadAsync(id);
        if (meal == null || meal.UserId != user.Id)
        {
            return NotFound();
        }

        await _mealRepo.LoadRecipesAsync(meal);

        return View(meal);
    }

    [HttpGet]
    public async Task<IActionResult> EditMeal(int id)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var meal = await _mealRepo.ReadAsync(id);
        if (meal == null || meal.UserId != user.Id)
        {
            return NotFound();
        }

        await _mealRepo.LoadRecipesAsync(meal);

        var viewModel = new EditMealViewModel
        {
            Id = meal.Id,
            Title = meal.Title,
            Date = meal.StartTime ?? DateTime.Today,
            Time = meal.StartTime.HasValue
                ? meal.StartTime.Value.ToString("HH:mm")
                : "00:00",
            SelectedMonth = meal.StartTime?.Month ?? DateTime.Today.Month,
            SelectedDay = meal.StartTime?.Day ?? DateTime.Today.Day,
            RepeatWeekly = meal.RepeatRule == "Weekly",
            RecipeIds = meal.Recipes?.Select(r => r.Id).ToList() ?? new List<int>(),
            Recipes = meal.Recipes?.Select(r => new RecipeDisplayViewModel
            {
                Id = r.Id,
                Name = r.Name
            }).ToList() ?? new List<RecipeDisplayViewModel>()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditMeal(EditMealViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
            return Challenge();

        var meal = await _context.Meals
            .Include(m => m.Recipes)
            .FirstOrDefaultAsync(m => m.Id == model.Id);

        if (meal == null || meal.UserId != user.Id)
            return NotFound();

        DateTime selectedDate = (model.Date == default)
            ? DateTime.Today
            : (TimeSpan.TryParse(model.Time, out var parsedTime)
                ? model.Date.Date + parsedTime
                : model.Date.Date);

        meal.Title = model.Title.Trim();
        meal.StartTime = selectedDate;
        meal.RepeatRule = model.RepeatWeekly ? "Weekly" : null;

        var incomingRecipeIds = model.RecipeIds
            .Distinct()
            .ToList();

        meal.Recipes.RemoveAll(r => !incomingRecipeIds.Contains(r.Id));

        foreach (var recipeId in incomingRecipeIds)
        {
            if (!meal.Recipes.Any(r => r.Id == recipeId))
            {
                var recipe = await _context.Recipes.FindAsync(recipeId);
                if (recipe != null)
                {
                    meal.Recipes.Add(recipe);
                }
            }
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Meal updated successfully";

        return RedirectToAction("EditMeal", new { id = meal.Id });
    }

    [HttpPost]
    public async Task<IActionResult> AddRecipeToMeal(int mealId, int recipeId)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        var meal = await _mealRepo.ReadAsync(mealId);
        if (meal == null || meal.UserId != user.Id) return NotFound();

        await _mealRepo.LoadRecipesAsync(meal);

        return RedirectToAction("EditMeal", new { id = meal.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMeal(int id, string? date)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var meal = await _context.Meals.FindAsync(id);
        if (meal == null)
        {
            return RedirectToAction("PlannerHome", "Meal", new { date });
        }

        if (meal.UserId != user.Id)
        {
            return Forbid();
        }

        _context.Meals.Remove(meal);
        await _context.SaveChangesAsync();

        return RedirectToAction("PlannerHome", "Meal", new { date });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRecipeFromMeal(int mealId, int recipeId)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        var meal = await _context.Meals
            .Include(m => m.Recipes)
            .FirstOrDefaultAsync(m => m.Id == mealId);

        if (meal == null || meal.UserId != user.Id) return NotFound();

        var recipe = meal.Recipes.FirstOrDefault(r => r.Id == recipeId);
        if (recipe != null)
        {
            meal.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMealCompleted(int id, string? date, bool? isCompleted)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var meal = await _context.Meals.FindAsync(id);
        if (meal == null)
        {
            return NotFound();
        }

        if (meal.UserId != user.Id)
        {
            return Forbid();
        }

        meal.IsCompleted = isCompleted == true;
        await _context.SaveChangesAsync();

        return RedirectToAction("PlannerHome", "Meal", new { date });
    }
}