using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IPantryRepository
{
    List<Ingredient> GetByUserId(string userId);
    void RemoveItem(int ingredientId, string userId);
    void UpdateItemAmount(int ingredientId, string userId, float newAmount);

    HashSet<int> GetUserIngredientBaseIds(string userId);
    List<Ingredient> GetMatchingPantryItems(string userId, HashSet<int> ingredientBaseIds);
    void AddItem(string userId, Ingredient item);

    void AddAutoRemovedIngredients(List<MealAutoRemovedIngredient> records);
    List<MealAutoRemovedIngredient> GetAutoRemovedIngredients(int mealId, DateTime completionDate);
    void RemoveAutoRemovedIngredients(int mealId, DateTime completionDate);
    void PurgeExpiredAutoRemovedIngredients(DateTime cutoff);
}
