using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class PantryService : IPantryService
{
    private readonly IUserRepository _userRepo;
    private readonly IMealAutoRemovedIngredientRepository _autoRemovedRepo;
    private readonly IIngredientBaseRepository _ingredientBaseRepo;
    private readonly IRepository<Measurement> _measurementRepo;

    public PantryService(
        IUserRepository userRepo,
        IMealAutoRemovedIngredientRepository autoRemovedRepo,
        IIngredientBaseRepository ingredientBaseRepo,
        IRepository<Measurement> measurementRepo)
    {
        _userRepo = userRepo;
        _autoRemovedRepo = autoRemovedRepo;
        _ingredientBaseRepo = ingredientBaseRepo;
        _measurementRepo = measurementRepo;
    }

    public List<Ingredient> GetPantryItems(string userId)
    {
        return _userRepo.GetByUserId(userId);
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
        _userRepo.RemoveItem(ingredientId, userId);
    }

    public void UpdatePantryItemAmount(int ingredientId, string userId, float newAmount)
    {
        _userRepo.UpdateItemAmount(ingredientId, userId, newAmount);
    }

    public HashSet<int> GetMealPantryMatchIds(string userId, List<Meal> meals)
    {
        var pantryBaseIds = _userRepo.GetUserIngredientBaseIds(userId);
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

        // Sum recipe amounts per ingredient base (handles multiple recipes using the same ingredient)
        var recipeInfoByBaseId = meal.Recipes
            .SelectMany(r => r.Ingredients)
            .GroupBy(i => i.IngredientBase.Id)
            .ToDictionary(
                g => g.Key,
                g => (Amount: g.Sum(i => i.Amount), MeasurementId: g.First().Measurement.Id));

        var matching = _userRepo.GetMatchingPantryItems(userId, recipeInfoByBaseId.Keys.ToHashSet());
        if (matching.Count == 0) return;

        var records = new List<MealAutoRemovedIngredient>();

        foreach (var item in matching)
        {
            if (!recipeInfoByBaseId.TryGetValue(item.IngredientBase.Id, out var recipeInfo))
                continue;

            bool sameUnit = item.Measurement.Id == recipeInfo.MeasurementId;

            if (sameUnit && item.Amount > recipeInfo.Amount)
            {
                // Pantry has more than the recipe needs — deduct only what was used
                records.Add(new MealAutoRemovedIngredient
                {
                    MealId = mealId,
                    CompletionDate = completionDate.Date,
                    IngredientBaseId = item.IngredientBase.Id,
                    DisplayName = item.DisplayName,
                    Amount = recipeInfo.Amount,
                    MeasurementId = item.Measurement.Id,
                    CreatedAt = DateTime.UtcNow
                });
                _userRepo.UpdateItemAmount(item.Id, userId, item.Amount - recipeInfo.Amount);
            }
            else
            {
                // Pantry has <= recipe amount, or different units — remove the whole row
                records.Add(new MealAutoRemovedIngredient
                {
                    MealId = mealId,
                    CompletionDate = completionDate.Date,
                    IngredientBaseId = item.IngredientBase.Id,
                    DisplayName = item.DisplayName,
                    Amount = item.Amount,
                    MeasurementId = item.Measurement.Id,
                    CreatedAt = DateTime.UtcNow
                });
                _userRepo.RemoveItem(item.Id, userId);
            }
        }

        _autoRemovedRepo.AddRange(records);
        await Task.CompletedTask;
    }

    public bool HasAutoRemovedIngredients(int mealId, DateTime completionDate)
    {
        return _autoRemovedRepo.GetByMealAndDate(mealId, completionDate).Count > 0;
    }

    public async Task RestorePantryItemsAsync(string userId, int mealId, DateTime completionDate)
    {
        var records = _autoRemovedRepo.GetByMealAndDate(mealId, completionDate);

        foreach (var record in records)
        {
            var item = new Ingredient
            {
                DisplayName = record.DisplayName,
                Amount = record.Amount,
                IngredientBase = record.IngredientBase,
                Measurement = record.Measurement
            };
            _userRepo.AddItem(userId, item);
        }

        _autoRemovedRepo.RemoveByMealAndDate(mealId, completionDate);

        await Task.CompletedTask;
    }
}
