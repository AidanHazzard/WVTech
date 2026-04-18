using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class ShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository;

    public ShoppingListService(IShoppingListRepository shoppingListRepository)
    {
        _shoppingListRepository = shoppingListRepository;
    }

    public void SyncFromMeals(string userId, IEnumerable<Ingredient> ingredients)
    {
        _shoppingListRepository.RemoveAutoAddedByUserId(userId);

        var grouped = ingredients
            .GroupBy(i => i.IngredientBase.Name.ToLower())
            .Select(g => new ShoppingListItem
            {
                UserId = userId,
                Name = g.First().IngredientBase.Name,
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
            Name = itemName.Trim(),
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

    public IEnumerable<ShoppingListItem> GetItemsForUser(string userId)
    {
        return _shoppingListRepository.GetByUserId(userId);
    }
}