using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IShoppingListRepository
{
    void Add(ShoppingListItem item);

    void Remove(int itemId, int userId);

    IEnumerable<ShoppingListItem> GetByUserId(int userId);
}