using MealPlanner.Models;

namespace MealPlanner.Services;

public interface IPantryService
{
    List<Ingredient> GetPantryItems(string userId);
    Ingredient BuildPantryItem(string name, float amount, string measurement);
    void RemovePantryItem(int ingredientId, string userId);
}
