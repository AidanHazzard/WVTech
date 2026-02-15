using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    public IActionResult PlannerHome()
    {
        return View();
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

        newMeal.User = await _userManager.GetUserAsync(User);

        _mealRepo.CreateOrUpdate(newMeal);
        _context.SaveChanges();

        return RedirectToAction("PlannerHome");
    }
}