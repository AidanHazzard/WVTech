using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers;

[Authorize]
public class MealController : Controller
{
    private readonly IRegistrationService _registrationService;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IMealRepository _mealRepo;
    private readonly MealPlannerDBContext _context;

    public MealController(
        IRegistrationService registrationService,
        IRecipeRepository recipeRepo,
        IMealRepository mealRepo,
        MealPlannerDBContext context)
    {
        _registrationService = registrationService;
        _recipeRepo = recipeRepo;
        _mealRepo = mealRepo;
        _context = context;
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
            Date = DateTime.Today
        });
    }

    [HttpPost]
    public async Task<IActionResult> NewMeal(CreateMealViewModel model)
    {
        Console.WriteLine(model.Date);
        Console.WriteLine(model.Time);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        Meal newMeal = new Meal
        {
            User = user,
            UserId = user.Id,
            Title = model.Title.Trim(),
            StartTime = model.Date.Date,
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

        return RedirectToAction("PlannerHome");
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

        var viewModel = new EditMealViewModel
        {
            Id = meal.Id,
            Date = meal.StartTime?.Date ?? DateTime.Today,
            Time = meal.StartTime?.TimeOfDay ?? TimeSpan.Zero,
            RepeatWeekly = meal.RepeatRule == "Weekly",
            RecipeIds = meal.Recipes?.Select(r => r.Id).ToList() ?? new List<int>()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditMeal(EditMealViewModel model)
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

        var meal = await _mealRepo.ReadAsync(model.Id);
        if (meal == null || meal.UserId != user.Id)
        {
            return NotFound();
        }

        meal.StartTime = model.Date.Date + model.Time;
        meal.RepeatRule = model.RepeatWeekly ? "Weekly" : null;

        meal.Recipes.Clear();

        foreach (var recipeId in model.RecipeIds ?? new List<int>())
        {
            var recipe = _recipeRepo.Read(recipeId);
            if (recipe != null)
            {
                meal.Recipes.Add(recipe);
            }
        }

        _mealRepo.CreateOrUpdate(meal);
        _context.SaveChanges();

        return RedirectToAction("ViewMeal", new { id = meal.Id });
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
            return RedirectToAction("Index", "Home", new { selectedDate = date });
        }

        if (meal.UserId != user.Id)
        {
            return Forbid();
        }

        _context.Meals.Remove(meal);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home", new { selectedDate = date });
    }
}