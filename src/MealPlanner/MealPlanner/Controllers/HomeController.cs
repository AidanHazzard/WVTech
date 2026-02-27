using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealPlanner.ViewModels;
using MealPlanner.Models;
using Microsoft.AspNetCore.Authorization;
using MealPlanner.Services;
using System.Security.Claims;
using MealPlanner.DAL.Abstract;

namespace MealPlanner.Controllers;

public class HomeController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly ILoginService _loginService;
    private readonly IRegistrationService _registrationService;
    private readonly IMealRepository _mealRepo;

    public HomeController(MealPlannerDBContext context, ILoginService loginService, IRegistrationService registrationService, IMealRepository mealRepo)
    {
        _context = context;
        _loginService = loginService;
        _registrationService = registrationService;
        _mealRepo = mealRepo;
    }

    public async Task<IActionResult> Index(string? date)
    {
        User user = await _registrationService.FindUserByClaimAsync(HttpContext.User);

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


    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    //This is the specific admin view
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }
    // This is the specific user view or dashboard
    [Authorize(Roles = "User")]
    public IActionResult User()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
