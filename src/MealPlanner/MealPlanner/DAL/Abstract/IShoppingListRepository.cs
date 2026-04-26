using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IShoppingListRepository
{
    void Add(ShoppingListItem item);

    void Remove(int itemId, string userId);

    void RemoveAllByName(string userId, string name);

    void RemoveAutoAddedByUserId(string userId);

    IEnumerable<ShoppingListItem> GetByUserId(string userId);

    void UpdateAmountByName(string userId, string name, float newAmount);
}