using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserRepository
{
    List<Ingredient> GetByUserId(string userId);
    void RemoveItem(int ingredientId, string userId);
    void UpdateItemAmount(int ingredientId, string userId, float newAmount);
    HashSet<int> GetUserIngredientBaseIds(string userId);
    List<Ingredient> GetMatchingPantryItems(string userId, HashSet<int> ingredientBaseIds);
    void AddItem(string userId, Ingredient item);
}
