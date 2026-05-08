using MealPlanner.Models;

namespace MealPlanner.Services;

public interface IShoppingListService
{
    IEnumerable<ShoppingListItem> GetItemsForUser(string userId);
    void AddItem(string userId, string itemName, float amount, string measurement);
    void RemoveItem(int itemId, string userId);
    void UpdateItemAmount(string userId, int ingredientBaseId, float newAmount);
    void SyncFromMeals(string userId, IEnumerable<Ingredient> ingredients);
}
