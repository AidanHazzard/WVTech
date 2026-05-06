using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IShoppingListRepository
{
    void Add(ShoppingListItem item);

    void Remove(int itemId, string userId);

    void RemoveAllByIngredientBase(string userId, int ingredientBaseId);

    void RemoveAutoAddedByUserId(string userId);

    IEnumerable<ShoppingListItem> GetByUserId(string userId);

    void UpdateAmountByIngredientBase(string userId, int ingredientBaseId, float newAmount);
}
