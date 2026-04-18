using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;

namespace MealPlanner.Controllers;

public class HomeController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly ILoginService _loginService;
    private readonly IRegistrationService _registrationService;
    private readonly IMealRepository _mealRepo;
    private readonly INutritionProgressService _nutritionProgressService;

    public HomeController(
        MealPlannerDBContext context,
        ILoginService loginService,
        IRegistrationService registrationService,
        IMealRepository mealRepo,
        INutritionProgressService nutritionProgressService)
    {
        _context = context;
        _loginService = loginService;
        _registrationService = registrationService;
        _mealRepo = mealRepo;
        _nutritionProgressService = nutritionProgressService;
    }

    public async Task<IActionResult> Index(string? date)
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            return Redirect("/Login");

        User? user = await _registrationService.FindUserByClaimAsync(HttpContext.User);

        if (user == null)
            return Redirect("/Login");

        DateTime selectedDate =
            DateTime.TryParse(date, out var parsed)
                ? parsed.Date
                : DateTime.Today;

        var meals = await _mealRepo.GetUserMealsByDateAsync(user, selectedDate);

        var nutrition = await _nutritionProgressService.GetDailyProgressAsync(
            user.Id,
            DateOnly.FromDateTime(selectedDate)
        );

        var bar = new NutritionBarInfoViewModel(
            nutrition.Totals.Calories, nutrition.Targets.Calories,
            nutrition.Totals.Protein,  nutrition.Targets.Protein,
            nutrition.Totals.Fat,      nutrition.Targets.Fat,
            nutrition.Totals.Carbs,    nutrition.Targets.Carbs
        );

        var vm = new PlannerHomeViewModel
        {
            SelectedDate = selectedDate,
            Meals = meals,
            NutritionBar = bar
        };

        return View(vm);
    }

    [Authorize]
    public Task<IActionResult> Dashboard(string? date)
    {
        return Index(date);
    }

    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}