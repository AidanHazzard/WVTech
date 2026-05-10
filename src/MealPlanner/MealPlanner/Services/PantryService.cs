using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class PantryService : IPantryService
{
    private readonly IPantryRepository _pantryRepo;
    private readonly IIngredientBaseRepository _ingredientBaseRepo;
    private readonly IRepository<Measurement> _measurementRepo;

    public PantryService(
        IPantryRepository pantryRepo,
        IIngredientBaseRepository ingredientBaseRepo,
        IRepository<Measurement> measurementRepo)
    {
        _pantryRepo = pantryRepo;
        _ingredientBaseRepo = ingredientBaseRepo;
        _measurementRepo = measurementRepo;
    }

    public List<Ingredient> GetPantryItems(string userId)
    {
        return _pantryRepo.GetByUserId(userId);
    }

    public Ingredient BuildPantryItem(string name, float amount, string measurement)
    {
        return new Ingredient
        {
            DisplayName = name,
            Amount = amount,
            IngredientBase = _ingredientBaseRepo.FindOrCreateByName(name),
            Measurement = _measurementRepo.FindOrCreate(
                m => m.Name == measurement,
                () => new Measurement { Name = measurement })
        };
    }

    public void RemovePantryItem(int ingredientId, string userId)
    {
        _pantryRepo.RemoveItem(ingredientId, userId);
    }

    public void UpdatePantryItemAmount(int ingredientId, string userId, float newAmount)
    {
        _pantryRepo.UpdateItemAmount(ingredientId, userId, newAmount);
    }

    public HashSet<int> GetMealPantryMatchIds(string userId, List<Meal> meals)
    {
        var pantryBaseIds = _pantryRepo.GetUserIngredientBaseIds(userId);
        if (pantryBaseIds.Count == 0) return [];

        var result = new HashSet<int>();
        foreach (var meal in meals)
        {
            var mealBaseIds = meal.Recipes
                .SelectMany(r => r.Ingredients)
                .Select(i => i.IngredientBase.Id)
                .ToHashSet();

            if (mealBaseIds.Overlaps(pantryBaseIds))
                result.Add(meal.Id);
        }
        return result;
    }

    public async Task AutoRemovePantryItemsAsync(string userId, int mealId, DateTime completionDate, List<Meal> meals)
    {
        var meal = meals.FirstOrDefault(m => m.Id == mealId);
        if (meal == null) return;

        var mealBaseIds = meal.Recipes
            .SelectMany(r => r.Ingredients)
            .Select(i => i.IngredientBase.Id)
            .ToHashSet();

        var matching = _pantryRepo.GetMatchingPantryItems(userId, mealBaseIds);
        if (matching.Count == 0) return;

        var records = matching.Select(item => new MealAutoRemovedIngredient
        {
            MealId = mealId,
            CompletionDate = completionDate.Date,
            IngredientBaseId = item.IngredientBase.Id,
            DisplayName = item.DisplayName,
            Amount = item.Amount,
            MeasurementId = item.Measurement.Id,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _pantryRepo.AddAutoRemovedIngredients(records);

        foreach (var item in matching)
            _pantryRepo.RemoveItem(item.Id, userId);

        await Task.CompletedTask;
    }

    public bool HasAutoRemovedIngredients(int mealId, DateTime completionDate)
    {
        return _pantryRepo.GetAutoRemovedIngredients(mealId, completionDate).Count > 0;
    }

    public async Task RestorePantryItemsAsync(string userId, int mealId, DateTime completionDate)
    {
        var records = _pantryRepo.GetAutoRemovedIngredients(mealId, completionDate);

        foreach (var record in records)
        {
            var item = new Ingredient
            {
                DisplayName = record.DisplayName,
                Amount = record.Amount,
                IngredientBase = record.IngredientBase,
                Measurement = record.Measurement
            };
            _pantryRepo.AddItem(userId, item);
        }

        _pantryRepo.RemoveAutoRemovedIngredients(mealId, completionDate);

        await Task.CompletedTask;
    }
}
