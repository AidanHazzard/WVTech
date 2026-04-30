using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class ShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository;
    private readonly IRepository<IngredientBase> _ingredientBaseRepo;
    private readonly IRepository<Measurement> _measurementRepo;

    public ShoppingListService(
        IShoppingListRepository shoppingListRepository,
        IRepository<IngredientBase> ingredientBaseRepo,
        IRepository<Measurement> measurementRepo)
    {
        _shoppingListRepository = shoppingListRepository;
        _ingredientBaseRepo = ingredientBaseRepo;
        _measurementRepo = measurementRepo;
    }

    public void SyncFromMeals(string userId, IEnumerable<Ingredient> ingredients)
    {
        _shoppingListRepository.RemoveAutoAddedByUserId(userId);

        var grouped = ingredients
            .GroupBy(i => (i.IngredientBase.Id, i.Measurement.Id))
            .Select(g => new ShoppingListItem
            {
                UserId = userId,
                IngredientBase = g.First().IngredientBase,
                Measurement = g.First().Measurement,
                DisplayName = g.First().DisplayName,
                Amount = g.Sum(i => i.Amount),
                IsAutoAdded = true
            });

        foreach (var item in grouped)
            _shoppingListRepository.Add(item);
    }

    public void AddItem(string userId, string itemName, float amount, string measurement)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("Item name cannot be empty.");

        var ingredientBase = _ingredientBaseRepo.FindOrCreate(
            b => b.Name == IngredientNameNormalizer.NormalizeKey(itemName),
            () => new IngredientBase { Name = IngredientNameNormalizer.NormalizeKey(itemName) });

        var measurementEntity = _measurementRepo.FindOrCreate(
            m => m.Name.ToLower() == measurement.Trim().ToLower(),
            () => new Measurement { Name = measurement.Trim() });

        var item = new ShoppingListItem
        {
            UserId = userId,
            IngredientBase = ingredientBase,
            Measurement = measurementEntity,
            DisplayName = itemName.Trim(),
            Amount = amount,
            IsAutoAdded = false
        };

        _shoppingListRepository.Add(item);
    }

    public void RemoveItem(int itemId, string userId)
    {
        if (itemId <= 0)
            throw new ArgumentException("Invalid item id.");

        _shoppingListRepository.Remove(itemId, userId);
    }

    public void RemoveItemsByIngredientBase(string userId, int ingredientBaseId)
    {
        _shoppingListRepository.RemoveAllByIngredientBase(userId, ingredientBaseId);
    }

    public void UpdateItemAmount(string userId, int ingredientBaseId, float newAmount)
    {
        if (newAmount < 0)
            throw new ArgumentException("Amount cannot be negative.");

        _shoppingListRepository.UpdateAmountByIngredientBase(userId, ingredientBaseId, newAmount);
    }

    public IEnumerable<ShoppingListItem> GetItemsForUser(string userId)
    {
        return _shoppingListRepository.GetByUserId(userId)
            .OrderBy(i => i.DisplayName)
            .ToList();
    }
}
