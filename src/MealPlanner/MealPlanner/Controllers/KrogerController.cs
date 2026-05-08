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
    private const string SessionToken = "KrogerAccessToken";
    private const string SessionTokenExpiry = "KrogerAccessTokenExpiry";
    private const string SessionPendingId = "KrogerPendingStoreId";
    private const string SessionPendingExp = "KrogerPendingExport";
    public const string SessionStoreId = "KrogerLastStoreId";

    private readonly IKrogerService? _krogerService;
    private readonly IKrogerExportService _exportService;
    private readonly IUserSettingsRepository _userSettingsRepo;
    private readonly ShoppingListService _shoppingListService;
    private readonly UserManager<User> _userManager;

    public KrogerController(
        IUserSettingsRepository userSettingsRepo,
        ShoppingListService shoppingListService,
        UserManager<User> userManager,
        IKrogerExportService exportService,
        IKrogerService? krogerService = null)
    {
        _userSettingsRepo = userSettingsRepo;
        _shoppingListService = shoppingListService;
        _userManager = userManager;
        _exportService = exportService;
        _krogerService = krogerService;
    }

    [HttpGet]
    public async Task<IActionResult> Stores(string zipCode)
    {
        if (_krogerService == null) return Json(Array.Empty<object>());
        return Json(await _krogerService.FindNearestStoresAsync(zipCode));
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

    [HttpGet]
    public async Task<IActionResult> ExportHistory()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        return Json(await _exportService.GetExportHistoryAsync(user.Id));
    }

    [HttpGet]
    public async Task<IActionResult> ExportDetail(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var detail = await _exportService.GetExportDetailAsync(id, user.Id);
        return detail == null ? NotFound() : Json(detail);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(string zipCode, string storeId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        await _userSettingsRepo.SaveZipCodeAsync(user.Id, zipCode);
        HttpContext.Session.SetString(SessionStoreId, storeId);

        if (_krogerService == null)
        {
            TempData["KrogerError"] = "Kroger integration is not configured.";
            return RedirectToAction("Index", "ShoppingList");
        }

        var krogerToken = GetValidSessionToken();
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
        if (_krogerService == null)
        {
            TempData["KrogerError"] = "Kroger integration is not configured.";
            return RedirectToAction("Index", "ShoppingList");
        }
        return Redirect(_krogerService.GetAuthorizationUrl("kroger-connect"));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(string code, string state)
    {
        if (_krogerService == null)
            return JsRedirect("/ShoppingList");

        var tokenResponse = await _krogerService.ExchangeCodeAsync(code);
        if (tokenResponse == null)
        {
            TempData["KrogerError"] = "Failed to connect to Kroger. Please try again.";
            return JsRedirect("/ShoppingList");
        }

        HttpContext.Session.SetString(SessionToken, tokenResponse.AccessToken);
        var expiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
        HttpContext.Session.SetString(SessionTokenExpiry, expiry.ToUnixTimeSeconds().ToString());

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

    private string? GetValidSessionToken()
    {
        var token = HttpContext.Session.GetString(SessionToken);
        if (string.IsNullOrEmpty(token)) return null;

        if (long.TryParse(HttpContext.Session.GetString(SessionTokenExpiry), out var expiry) &&
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiry)
        {
            HttpContext.Session.Remove(SessionToken);
            HttpContext.Session.Remove(SessionTokenExpiry);
            return null;
        }

        return token;
    }

    private async Task<IActionResult> PerformExport(string userId, string storeId, string krogerToken, bool isRetry = false)
    {
        var result = await _exportService.RunExportAsync(userId, storeId, krogerToken);

        switch (result.Outcome)
        {
            case KrogerExportOutcome.NoItems:
                TempData["KrogerInfo"] = "Kroger account connected! Add items to your shopping list and click Export to send them to your cart.";
                return RedirectToAction("Index", "ShoppingList");

            case KrogerExportOutcome.SearchTokenFailed:
                TempData["KrogerError"] = "Could not connect to Kroger. Please try again.";
                return RedirectToAction("Index", "ShoppingList");

            case KrogerExportOutcome.NoMatchesFound:
                TempData["KrogerError"] = "No matching Kroger products found for your shopping list items.";
                return RedirectToAction("Index", "ShoppingList");

            case KrogerExportOutcome.Success:
                HttpContext.Response.Cookies.Append("ShoppingListSynced", "1", new CookieOptions { HttpOnly = true });
                TempData["KrogerSuccess"] = result.Skipped.Count == 0
                    ? $"{result.ItemsAdded} item(s) added to your Kroger cart!"
                    : $"{result.ItemsAdded} item(s) added to your Kroger cart. Could not find: {string.Join(", ", result.Skipped)}.";
                return RedirectToAction("Index", "ShoppingList");

            default: // ExportFailed
                HttpContext.Session.Remove(SessionToken);
                TempData["KrogerError"] = isRetry
                    ? "Error connecting to your Kroger account. Please try again later."
                    : "Your Kroger session expired. Click Export to Kroger again to reconnect.";
                return RedirectToAction("Index", "ShoppingList");
        }
    }

    private ViewResult JsRedirect(string url) => View("Redirect", url);
}
