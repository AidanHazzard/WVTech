using MealPlanner.Models;

namespace MealPlanner.Services;

public interface IPantryService
{
    List<Ingredient> GetPantryItems(string userId);
    Ingredient BuildPantryItem(string name, float amount, string measurement);
    void RemovePantryItem(int ingredientId, string userId);
    void UpdatePantryItemAmount(int ingredientId, string userId, float newAmount);

    HashSet<int> GetMealPantryMatchIds(string userId, List<Meal> meals);
    Task AutoRemovePantryItemsAsync(string userId, int mealId, DateTime completionDate, List<Meal> meals);
    bool HasAutoRemovedIngredients(int mealId, DateTime completionDate);
    Task RestorePantryItemsAsync(string userId, int mealId, DateTime completionDate);
}
