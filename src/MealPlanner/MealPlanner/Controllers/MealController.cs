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
}
