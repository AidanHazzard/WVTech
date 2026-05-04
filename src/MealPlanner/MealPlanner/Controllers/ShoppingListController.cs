using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers;

[Authorize]
public class ShoppingListController : Controller
{
    private readonly ShoppingListService _shoppingListService;
    private readonly UserManager<User> _userManager;
    private readonly IMealRepository _mealRepo;
    private readonly MealPlannerDBContext _context;
    private readonly IUserSettingsRepository _userSettingsRepo;

    public ShoppingListController(
        ShoppingListService shoppingListService,
        UserManager<User> userManager,
        IMealRepository mealRepo,
        MealPlannerDBContext context,
        IUserSettingsRepository userSettingsRepo)
    {
        _shoppingListService = shoppingListService;
        _userManager = userManager;
        _mealRepo = mealRepo;
        _context = context;
        _userSettingsRepo = userSettingsRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        DateTime dateFrom = DateTime.Today;
        DateTime dateTo = DateTime.Today;

        if (Request.Cookies.TryGetValue("ShoppingListDateFrom", out var fromStr) &&
            Request.Cookies.TryGetValue("ShoppingListDateTo", out var toStr) &&
            DateTime.TryParse(fromStr, out var cookieFrom) &&
            DateTime.TryParse(toStr, out var cookieTo) &&
            cookieTo >= DateTime.Today)
        {
            dateFrom = cookieFrom;
            dateTo = cookieTo;
        }

        if (!Request.Cookies.ContainsKey("ShoppingListSynced"))
        {
            await SyncMealIngredients(user, dateFrom, dateTo);
            Response.Cookies.Append("ShoppingListSynced", "1", new CookieOptions { HttpOnly = true });
        }

        var items = _shoppingListService.GetItemsForUser(user.Id);
        var profile = await _userSettingsRepo.GetByUserIdAsync(user.Id);

        return View(new ShoppingListViewModel
        {
            Items = items,
            DateFrom = dateFrom,
            DateTo = dateTo,
            ZipCode = profile?.ZipCode,
            KrogerConnected = !string.IsNullOrEmpty(HttpContext.Session.GetString("KrogerAccessToken"))
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetDateRange(DateTime dateFrom, DateTime dateTo)
    {
        if (dateFrom > dateTo)
            dateTo = dateFrom;

        var rangeOptions = new CookieOptions { Expires = dateTo.AddDays(1), HttpOnly = true };
        Response.Cookies.Append("ShoppingListDateFrom", dateFrom.ToString("yyyy-MM-dd"), rangeOptions);
        Response.Cookies.Append("ShoppingListDateTo", dateTo.ToString("yyyy-MM-dd"), rangeOptions);

        Response.Cookies.Delete("ShoppingListSynced");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(string itemName, float amount, string measurement)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            _shoppingListService.AddItem(user.Id, itemName, amount, measurement);
        }
        catch (ArgumentException ex)
        {
            TempData["ShoppingListError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItemAmount(string itemName, float newAmount)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            _shoppingListService.UpdateItemAmount(user.Id, itemName, newAmount);
        }
        catch (ArgumentException ex)
        {
            TempData["ShoppingListError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(string itemName)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            _shoppingListService.RemoveItemsByName(user.Id, itemName);
        }
        catch (ArgumentException ex)
        {
            TempData["ShoppingListError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SyncMealIngredients(User user, DateTime dateFrom, DateTime dateTo)
    {
        var meals = await _mealRepo.GetUserMealsByDateRangeAsync(user, dateFrom, dateTo);

        var recipeIds = meals.SelectMany(m => m.Recipes).Select(r => r.Id).Distinct().ToList();
        if (recipeIds.Count > 0)
            await _context.Set<Recipe>()
                .Where(r => recipeIds.Contains(r.Id))
                .Include(r => r.Ingredients)
                .LoadAsync();

        var ingredients = meals
            .SelectMany(m => m.Recipes)
            .SelectMany(r => r.Ingredients)
            .ToList();

        _shoppingListService.SyncFromMeals(user.Id, ingredients);
    }
}
