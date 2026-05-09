using MealPlanner.Models;

namespace MealPlanner.Services;


public interface IShoppingListService
{
    IEnumerable<ShoppingListItem> GetItemsForUser(string userId);
    void AddItem(string userId, string itemName, float amount, string measurement);
    void RemoveItem(int itemId, string userId);
    void UpdateItemAmount(string userId, int ingredientBaseId, float newAmount);
    Task SyncFromDateRangeAsync(string userId, User user, DateTime dateFrom, DateTime dateTo);
}
