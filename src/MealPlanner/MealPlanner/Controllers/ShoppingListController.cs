using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Controllers;

[Authorize]
public class ShoppingListController : Controller
{
    private readonly ShoppingListService _shoppingListService;
    private readonly UserManager<User> _userManager;

    public ShoppingListController(ShoppingListService shoppingListService, UserManager<User> userManager)
    {
        _shoppingListService = shoppingListService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var items = _shoppingListService.GetItemsForUser(user.Id);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(string itemName)
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        try
        {
            _shoppingListService.AddItem(user.Id, itemName);
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

        if (user == null)
        {
            return Challenge();
        }

        try
        {
            _shoppingListService.RemoveItem(itemId, user.Id);
        }
        catch (ArgumentException ex)
        {
            TempData["ShoppingListError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}