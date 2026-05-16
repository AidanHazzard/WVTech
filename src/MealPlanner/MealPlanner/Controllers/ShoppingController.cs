using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace MealPlanner.Controllers;

[Authorize]
public class ShoppingController : Controller
{
    private readonly IShoppingListService _shoppingListService;
    private readonly IPantryService _pantryService;
    private readonly UserManager<User> _userManager;
    private readonly IUserSettingsRepository _userSettingsRepo;
    private readonly IRegistrationService _registrationService;
    private readonly MealPlannerDBContext _context;

    public ShoppingController(
        IShoppingListService shoppingListService,
        IPantryService pantryService,
        UserManager<User> userManager,
        IUserSettingsRepository userSettingsRepo,
        IRegistrationService registrationService,
        MealPlannerDBContext context)
    {
        _shoppingListService = shoppingListService;
        _pantryService = pantryService;
        _userManager = userManager;
        _userSettingsRepo = userSettingsRepo;
        _registrationService = registrationService;
        _context = context;
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
            await _shoppingListService.SyncFromDateRangeAsync(user.Id, user, dateFrom, dateTo);
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
            LastStoreId = HttpContext.Session.GetString(KrogerController.SessionStoreId),
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
    public async Task<IActionResult> AddItem(string itemName, string amount, string measurement)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        float? parsedAmount = FractionParser.ParseAmount(amount);
        if (parsedAmount == null)
        {
            TempData["ShoppingListError"] = $"Invalid amount \"{amount}\". Use a number, fraction (1/2), or mixed number (1 1/2).";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _shoppingListService.AddItem(user.Id, itemName, parsedAmount.Value, measurement);
            TempData["ShoppingListSuccess"] = $"{itemName} added to your shopping list.";
        }
        catch (ArgumentException ex)
        {
            TempData["ShoppingListError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItemAmount(int ingredientBaseId, string newAmount)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        float parsedAmount = FractionParser.ParseAmount(newAmount) ?? 0f;
        try
        {
            _shoppingListService.UpdateItemAmount(user.Id, ingredientBaseId, parsedAmount);
        }
        catch (ArgumentException ex)
        {
            TempData["ShoppingListError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        _shoppingListService.RemoveItem(itemId, user.Id);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Pantry()
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        var items = _pantryService.GetPantryItems(user.Id);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePantryItemAmount(int ingredientId, float? newAmount)
    {
        if (newAmount == null || newAmount <= 0)
        {
            TempData["ValidationError"] = "Amount must be greater than zero.";
            return RedirectToAction(nameof(Pantry));
        }

        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        _pantryService.UpdatePantryItemAmount(ingredientId, user.Id, newAmount.Value);
        _context.SaveChanges();

        return RedirectToAction(nameof(Pantry));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePantryItem(int ingredientId)
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        _pantryService.RemovePantryItem(ingredientId, user.Id);
        _context.SaveChanges();

        return RedirectToAction(nameof(Pantry));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPantryItem(PantryItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ValidationError"] = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors.";
            return RedirectToAction(nameof(Pantry));
        }

        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        var ingredient = _pantryService.BuildPantryItem(model.Name, model.Amount, model.Measurement);
        _pantryService.AddPantryItem(user.Id, ingredient);
        _context.SaveChanges();

        TempData["SuccessMessage"] = $"{model.Name} was added to your pantry.";
        return RedirectToAction(nameof(Pantry));
    }

}
