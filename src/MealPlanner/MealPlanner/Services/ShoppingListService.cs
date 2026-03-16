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

    public void AddItem(string userId, string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            throw new ArgumentException("Item name cannot be empty.");
        }

        var item = new ShoppingListItem
        {
            UserId = userId,
            Name = itemName.Trim()
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