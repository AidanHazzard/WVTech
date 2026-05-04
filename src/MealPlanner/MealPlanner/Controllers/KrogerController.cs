using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Models.DTO;
using MealPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Controllers;

[Authorize]
public class KrogerController : Controller
{
    private const string SessionToken      = "KrogerAccessToken";
    private const string SessionPendingId  = "KrogerPendingStoreId";
    private const string SessionPendingExp = "KrogerPendingExport";

    private readonly IKrogerService? _krogerService;
    private readonly IUserSettingsRepository _userSettingsRepo;
    private readonly ShoppingListService _shoppingListService;
    private readonly UserManager<User> _userManager;

    public KrogerController(
        IUserSettingsRepository userSettingsRepo,
        ShoppingListService shoppingListService,
        UserManager<User> userManager,
        IKrogerService? krogerService = null)
    {
        _userSettingsRepo = userSettingsRepo;
        _shoppingListService = shoppingListService;
        _userManager = userManager;
        _krogerService = krogerService;
    }

    [HttpGet]
    public async Task<IActionResult> Stores(string zipCode)
    {
        if (_krogerService == null) return Json(Array.Empty<object>());
        var stores = await _krogerService.FindNearestStoresAsync(zipCode);
        return Json(stores);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SaveZip([FromBody] SaveZipRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        await _userSettingsRepo.SaveZipCodeAsync(user.Id, request.ZipCode);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(string zipCode, string storeId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        await _userSettingsRepo.SaveZipCodeAsync(user.Id, zipCode);

        if (KrogerNotConfigured(out var notConfiguredResult)) return notConfiguredResult;

        var krogerToken = HttpContext.Session.GetString(SessionToken);
        if (string.IsNullOrEmpty(krogerToken))
        {
            if (_shoppingListService.GetItemsForUser(user.Id).Any())
            {
                HttpContext.Session.SetString(SessionPendingId, storeId);
                HttpContext.Session.SetString(SessionPendingExp, "true");
            }
            return RedirectToAction(nameof(Connect));
        }

        return await PerformExport(user.Id, storeId, krogerToken);
    }

    [HttpGet]
    public IActionResult Connect()
    {
        if (KrogerNotConfigured(out var notConfiguredResult)) return notConfiguredResult;
        return Redirect(_krogerService!.GetAuthorizationUrl("kroger-connect"));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(string code, string state)
    {
        if (KrogerNotConfigured(out _))
            return JsRedirect("/ShoppingList");

        var tokenResponse = await _krogerService!.ExchangeCodeAsync(code);
        if (tokenResponse == null)
        {
            TempData["KrogerError"] = "Failed to connect to Kroger. Please try again.";
            return JsRedirect("/ShoppingList");
        }

        HttpContext.Session.SetString(SessionToken, tokenResponse.AccessToken);

        if (HttpContext.Session.GetString(SessionPendingExp) == "true")
        {
            HttpContext.Session.Remove(SessionPendingExp);
            var pendingStoreId = HttpContext.Session.GetString(SessionPendingId);
            HttpContext.Session.Remove(SessionPendingId);
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(pendingStoreId))
                return await PerformExport(user.Id, pendingStoreId, tokenResponse.AccessToken, isRetry: true);
        }

        TempData["KrogerInfo"] = "Kroger account connected! Click Export to Kroger to add your items.";
        return JsRedirect("/ShoppingList");
    }

    private bool KrogerNotConfigured(out IActionResult result)
    {
        if (_krogerService != null) { result = null!; return false; }
        TempData["KrogerError"] = "Kroger integration is not configured.";
        result = RedirectToAction("Index", "ShoppingList");
        return true;
    }

    private ViewResult JsRedirect(string url) => View("Redirect", url);

    private async Task<IActionResult> PerformExport(string userId, string storeId, string krogerToken, bool isRetry = false)
    {
        var items = _shoppingListService.GetItemsForUser(userId).ToList();
        if (items.Count == 0)
        {
            TempData["KrogerInfo"] = "Kroger account connected! Add items to your shopping list and click Export to send them to your cart.";
            return RedirectToAction("Index", "ShoppingList");
        }

        var cartItems = new List<KrogerCartItem>();
        foreach (var item in items)
        {
            var match = await _krogerService!.SearchProductUpcAsync(
                item.Name, storeId, item.Amount, item.Measurement);
            if (match != null)
                cartItems.Add(new KrogerCartItem { Upc = match.Upc, Quantity = match.Quantity });
        }

        if (cartItems.Count == 0)
        {
            TempData["KrogerError"] = "No matching Kroger products found for your shopping list items.";
            return RedirectToAction("Index", "ShoppingList");
        }

        var success = await _krogerService!.ExportCartAsync(cartItems, krogerToken);
        if (success)
        {
            TempData["KrogerSuccess"] = $"{cartItems.Count} item(s) added to your Kroger cart!";
            return RedirectToAction("Index", "ShoppingList");
        }

        HttpContext.Session.Remove(SessionToken);
        TempData["KrogerError"] = isRetry
            ? "Error connecting to your Kroger account. Please try again later."
            : "Your Kroger session expired. Click Export to Kroger again to reconnect.";
        return RedirectToAction("Index", "ShoppingList");
    }
}
