using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IIngredientBaseRepository : IRepository<IngredientBase>
{
    IngredientBase FindOrCreateByName(string name);
}
