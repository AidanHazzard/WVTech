using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IPantryRepository
{
    List<Ingredient> GetByUserId(string userId);
    void RemoveItem(int ingredientId, string userId);
}
