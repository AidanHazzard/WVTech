using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers;

[Authorize]
public class MealController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IRepository<Meal> _mealRepo;
    private readonly MealPlannerDBContext _context;

    public MealController(UserManager<User> userManager, IRecipeRepository recipeRepo, IRepository<Meal> mealRepo, MealPlannerDBContext context)
    {
        _userManager = userManager;
        _recipeRepo = recipeRepo;
        _mealRepo = mealRepo;
        _context = context;
    }

    public async Task<IActionResult> PlannerHome(string? date)
{
    var user = await _userManager.GetUserAsync(User);

    DateTime selectedDate =
        DateTime.TryParse(date, out var parsed)
            ? parsed.Date
            : DateTime.Today;

    var start = selectedDate;
    var end = selectedDate.AddDays(1);

    var exactDateMeals = await _context.Set<Meal>()
        .Include(m => m.Recipes)
        .Where(m => m.UserId == user.Id && m.StartTime != null)
        .Where(m => m.StartTime >= start && m.StartTime < end)
        .ToListAsync();

    var weeklyRepeatMeals = await _context.Set<Meal>()
        .Include(m => m.Recipes)
        .Where(m => m.UserId == user.Id && m.StartTime != null)
        .Where(m => m.RepeatRule == "Weekly")
        .ToListAsync();

    weeklyRepeatMeals = weeklyRepeatMeals
        .Where(m => m.StartTime!.Value.DayOfWeek == selectedDate.DayOfWeek)
        .ToList();

    var meals = exactDateMeals
        .Concat(weeklyRepeatMeals)
        .GroupBy(m => m.Id)
        .Select(g => g.First())
        .OrderBy(m => m.StartTime)
        .ToList();

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

        var user = await _userManager.GetUserAsync(User);
        newMeal.User = user;
        newMeal.UserId = user.Id;

        newMeal.StartTime = model.Date.Date.Add(model.Time);
        newMeal.RepeatRule = model.RepeatWeekly ? "Weekly" : null;

        _mealRepo.CreateOrUpdate(newMeal);
        _context.SaveChanges();

        return RedirectToAction("PlannerHome");
    }
}
