using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}