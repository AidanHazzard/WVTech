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

    public MealController(IRegistrationService registrationService, IRecipeRepository recipeRepo, IMealRepository mealRepo, MealPlannerDBContext context)
    {
        _registrationService = registrationService;
        _recipeRepo = recipeRepo;
        _mealRepo = mealRepo;
        _context = context;
    }

    public async Task<IActionResult> PlannerHome(string? date)
    {
        User user = await _registrationService.FindUserByClaimAsync(User);

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
        return View();
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

        Meal newMeal = new Meal();

        foreach (int i in model.RecipeIds)
        {
            Recipe recipe = _recipeRepo.Read(i);
            newMeal.Recipes.Add(recipe);
        }

        User user = await _registrationService.FindUserByClaimAsync(User);
        newMeal.User = user;
        newMeal.UserId = user.Id;

        newMeal.StartTime = model.Date.Date.Add(model.Time);
        newMeal.RepeatRule = model.RepeatWeekly ? "Weekly" : null;

        _mealRepo.CreateOrUpdate(newMeal);
        _context.SaveChanges();

        return RedirectToAction("PlannerHome");
    }

    [HttpGet]
    public async Task<IActionResult> ViewMeal(int id)
    {
        User user = await _registrationService.FindUserByClaimAsync(User);

        var meal = await _mealRepo.ReadAsync(id);
        if (meal == null || meal.UserId != user.Id)
            return NotFound();

        await _mealRepo.LoadRecipesAsync(meal);

        return View(meal);
    }


    [HttpGet]
    public async Task<IActionResult> EditMeal(int id)
    {
        var meal = await _mealRepo.ReadAsync(id);
        if (meal == null) return NotFound();

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

        var meal = await _mealRepo.ReadAsync(model.Id);
        if (meal == null) return NotFound();

        meal.StartTime = model.Date.Date + model.Time;
        meal.RepeatRule = model.RepeatWeekly ? "Weekly" : null;

        // Clear old recipes
        meal.Recipes.Clear();

        foreach (var recipeId in model.RecipeIds ?? new List<int>())
        {
            var recipe = _recipeRepo.Read(recipeId); // <- sync version
            if (recipe != null)
                meal.Recipes.Add(recipe);
        }

        _mealRepo.CreateOrUpdate(meal);
        _context.SaveChanges(); // sync save

        return RedirectToAction("ViewMeal", new { id = meal.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteMeal(int id)
    {
        var meal = _mealRepo.Read(id);
        if (meal == null)
        {
            return NotFound();
        }

        _mealRepo.Delete(meal);
        _context.SaveChanges(); // or _context.SaveChanges() if you prefer

        // Redirect to your main page after deletion
        return RedirectToAction("Index", "Home");
    }
}
