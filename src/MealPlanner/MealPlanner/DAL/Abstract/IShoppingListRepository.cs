using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IShoppingListRepository
{
    void Add(ShoppingListItem item);

    void Remove(int itemId, string userId);

    IEnumerable<ShoppingListItem> GetByUserId(string userId);
}