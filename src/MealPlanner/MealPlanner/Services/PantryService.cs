using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class PantryService : IPantryService
{
    private readonly IPantryRepository _pantryRepo;
    private readonly IIngredientBaseRepository _ingredientBaseRepo;
    private readonly IRepository<Measurement> _measurementRepo;

    public PantryService(
        IPantryRepository pantryRepo,
        IIngredientBaseRepository ingredientBaseRepo,
        IRepository<Measurement> measurementRepo)
    {
        _pantryRepo = pantryRepo;
        _ingredientBaseRepo = ingredientBaseRepo;
        _measurementRepo = measurementRepo;
    }

    public List<Ingredient> GetPantryItems(string userId)
    {
        return _pantryRepo.GetByUserId(userId);
    }

    public Ingredient BuildPantryItem(string name, float amount, string measurement)
    {
        return new Ingredient
        {
            DisplayName = name,
            Amount = amount,
            IngredientBase = _ingredientBaseRepo.FindOrCreateByName(name),
            Measurement = _measurementRepo.FindOrCreate(
                m => m.Name == measurement,
                () => new Measurement { Name = measurement })
        };
    }

    public void RemovePantryItem(int ingredientId, string userId)
    {
        _pantryRepo.RemoveItem(ingredientId, userId);
    }
}
