using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers;

[Authorize]
public class PantryController : Controller
{
    private readonly IRegistrationService _registrationService;
    private readonly MealPlannerDBContext _context;
    private readonly IRepository<IngredientBase> _ingredientBaseRepo;
    private readonly IRepository<Measurement> _measurementRepo;

    public PantryController(
        IRegistrationService registrationService,
        MealPlannerDBContext context,
        IRepository<IngredientBase> ingredientBaseRepo,
        IRepository<Measurement> measurementRepo)
    {
        _registrationService = registrationService;
        _context = context;
        _ingredientBaseRepo = ingredientBaseRepo;
        _measurementRepo = measurementRepo;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        var items = await _context.Users
            .Where(u => u.Id == user.Id)
            .SelectMany(u => u.PantryItems)
            .Include(i => i.IngredientBase)
            .Include(i => i.Measurement)
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(PantryItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ValidationError"] = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors.";
            return RedirectToAction("Index");
        }

        var user = await _registrationService.FindUserByClaimAsync(User);
        if (user == null) return Challenge();

        var ingredient = ViewModelService.IngredientFromPantryItemVM(model);
        ingredient.IngredientBase = _ingredientBaseRepo.FindOrCreate(
            b => b.Name == IngredientNameNormalizer.NormalizeKey(model.Name),
            () => new IngredientBase { Name = IngredientNameNormalizer.NormalizeKey(model.Name) });
        ingredient.Measurement = _measurementRepo.FindOrCreate(
            m => m.Name == model.Measurement,
            () => new Measurement { Name = model.Measurement });

        user.PantryItems.Add(ingredient);
        _context.SaveChanges();

        TempData["SuccessMessage"] = $"{model.Name} was added to your pantry.";
        return RedirectToAction("Index");
    }
}
