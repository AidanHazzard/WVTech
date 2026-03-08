using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly MealPlannerDBContext _context;
    private readonly IUserRecipeRepository _userRecipeRepository;
    private readonly IRegistrationService _registrationService;
    private readonly IRecipeRepository _recipeRepository;

    public FavoritesController(
        MealPlannerDBContext context, 
        IUserRecipeRepository userRecipeRepository, 
        IRegistrationService registrationService,
        IRecipeRepository recipeRepository)
    {
        _context = context;
        _userRecipeRepository = userRecipeRepository;
        _registrationService = registrationService;
        _recipeRepository = recipeRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Add(int recipeId, string? returnUrl = null)
    {
        User? user = await _registrationService.FindUserByClaimAsync(User);
        Recipe? recipe = _recipeRepository.Read(recipeId);
        if (user == null) return Unauthorized();
        
        await _userRecipeRepository.AddFavoriteAsync(user, recipe);
        await _context.SaveChangesAsync();
        
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("MyFavorites");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int recipeId, string? returnUrl = null)
    {
        User? user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Unauthorized();

        await _userRecipeRepository.RemoveFavoriteAsync(user.Id, recipeId);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("MyFavorites");
    }

    [HttpGet]
    public async Task<IActionResult> MyFavorites()
    {
        User? user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Unauthorized();
        
        List<Recipe> favorites = await _userRecipeRepository.GetFavoritesAsync(user.Id);
        return View(favorites);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(MyFavorites));
    }
}