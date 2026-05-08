using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class ShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository;
    private readonly IMealRepository _mealRepository;

    public ShoppingListService(IShoppingListRepository shoppingListRepository, IMealRepository mealRepository)
    {
        _shoppingListRepository = shoppingListRepository;
        _mealRepository = mealRepository;
    }

    public async Task SyncFromDateRangeAsync(string userId, User user, DateTime dateFrom, DateTime dateTo)
    {
        var meals = await _mealRepository.GetUserMealsByDateRangeWithIngredientsAsync(user, dateFrom, dateTo);
        var ingredients = meals.SelectMany(m => m.Recipes).SelectMany(r => r.Ingredients).ToList();
        SyncFromMeals(userId, ingredients);
    }

    private void SyncFromMeals(string userId, IEnumerable<Ingredient> ingredients)
    {
        _shoppingListRepository.RemoveAutoAddedByUserId(userId);

        var grouped = ingredients
            .GroupBy(i => (IngredientNameNormalizer.NormalizeKey(i.IngredientBase.Name), i.Measurement.Name.ToLower()))
            .Select(g => new ShoppingListItem
            {
                UserId = userId,
                Name = IngredientNameNormalizer.Normalize(g.First().IngredientBase.Name),
                Amount = g.Sum(i => i.Amount),
                Measurement = g.First().Measurement.Name,
                IsAutoAdded = true
            });

        foreach (var item in grouped)
            _shoppingListRepository.Add(item);
    }

    public void AddItem(string userId, string itemName, float amount, string measurement)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            throw new ArgumentException("Item name cannot be empty.");
        }

        var item = new ShoppingListItem
        {
            UserId = userId,
            Name = IngredientNameNormalizer.Normalize(itemName),
            Amount = amount,
            Measurement = measurement.Trim()
        };

        _shoppingListRepository.Add(item);
    }

    public void RemoveItem(int itemId, string userId)
    {
        if (itemId <= 0)
        {
            throw new ArgumentException("Invalid item id.");
        }

        _shoppingListRepository.Remove(itemId, userId);
    }

    public void RemoveItemsByName(string userId, string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            throw new ArgumentException("Item name cannot be empty.");
        }

        _shoppingListRepository.RemoveAllByName(userId, itemName.Trim());
    }

    public void UpdateItemAmount(string userId, string itemName, float newAmount)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("Item name cannot be empty.");

        if (newAmount < 0)
            throw new ArgumentException("Amount cannot be negative.");

        _shoppingListRepository.UpdateAmountByName(userId, itemName.Trim(), newAmount);
    }

    public void ClearItems(string userId)
    {
        _shoppingListRepository.ClearAllItems(userId);
    }

    public IEnumerable<ShoppingListItem> GetItemsForUser(string userId)
    {
        return _shoppingListRepository.GetByUserId(userId)
            .GroupBy(i => (IngredientNameNormalizer.NormalizeKey(i.Name), i.Measurement.ToLower()))
            .Select(g => new ShoppingListItem
            {
                UserId = userId,
                Name = IngredientNameNormalizer.Normalize(g.First().Name),
                Amount = g.Sum(i => i.Amount),
                Measurement = g.First().Measurement,
                IsAutoAdded = g.Any(i => i.IsAutoAdded)
            })
            .OrderBy(i => i.Name)
            .ToList();
    }
}