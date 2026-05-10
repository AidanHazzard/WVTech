using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IMealAutoRemovedIngredientRepository
{
    void AddRange(List<MealAutoRemovedIngredient> records);
    List<MealAutoRemovedIngredient> GetByMealAndDate(int mealId, DateTime completionDate);
    void RemoveByMealAndDate(int mealId, DateTime completionDate);
    void PurgeExpired(DateTime cutoff);
}
