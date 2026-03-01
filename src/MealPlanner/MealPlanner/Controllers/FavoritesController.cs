using MealPlanner.Models;
using MealPlanner.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly IFavoritesService _favoritesService;
    private readonly UserManager<User> _userManager;

    public FavoritesController(IFavoritesService favoritesService, UserManager<User> userManager)
    {
        _favoritesService = favoritesService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Add(int recipeId, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _favoritesService.AddFavoriteAsync(user.Id, recipeId);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("MyFavorites");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int recipeId, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _favoritesService.RemoveFavoriteAsync(user.Id, recipeId);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("MyFavorites");
    }

    [HttpGet]
    public async Task<IActionResult> MyFavorites()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var favorites = await _favoritesService.GetFavoritesAsync(user.Id);
        return View(favorites);
    }
}