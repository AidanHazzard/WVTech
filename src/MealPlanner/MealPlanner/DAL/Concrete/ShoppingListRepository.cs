using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class ShoppingListRepository : IShoppingListRepository
{
    private readonly MealPlannerDBContext _context;

    public ShoppingListRepository(MealPlannerDBContext context)
    {
        _context = context;
    }

    public void Add(ShoppingListItem item)
    {
        var existing = _context.ShoppingListItems
            .FirstOrDefault(i => i.UserId == item.UserId
                              && i.IngredientBaseId == item.IngredientBase.Id
                              && i.MeasurementId == item.Measurement.Id
                              && i.IsAutoAdded == item.IsAutoAdded);

        if (existing != null)
        {
            existing.Amount += item.Amount;
        }
        else
        {
            _context.ShoppingListItems.Add(item);
        }

        _context.SaveChanges();
    }

    public void Remove(int itemId, string userId)
    {
        ShoppingListItem? item = _context.ShoppingListItems
            .FirstOrDefault(i => i.Id == itemId && i.UserId == userId);

        if (item != null)
        {
            _context.ShoppingListItems.Remove(item);
            _context.SaveChanges();
        }
    }

    public void RemoveAllByIngredientBase(string userId, int ingredientBaseId)
    {
        var items = _context.ShoppingListItems
            .Where(i => i.UserId == userId && i.IngredientBaseId == ingredientBaseId)
            .ToList();

        if (items.Count > 0)
        {
            _context.ShoppingListItems.RemoveRange(items);
            _context.SaveChanges();
        }
    }

    public void RemoveAutoAddedByUserId(string userId)
    {
        var items = _context.ShoppingListItems
            .Where(i => i.UserId == userId && i.IsAutoAdded)
            .ToList();

        _context.ShoppingListItems.RemoveRange(items);
        _context.SaveChanges();
    }

    public IEnumerable<ShoppingListItem> GetByUserId(string userId)
    {
        return _context.ShoppingListItems
            .Where(i => i.UserId == userId)
            .OrderBy(i => i.IngredientBase.Name)
            .ToList();
    }

    public void UpdateAmountByIngredientBase(string userId, int ingredientBaseId, float newAmount)
    {
        var items = _context.ShoppingListItems
            .Where(i => i.UserId == userId && i.IngredientBaseId == ingredientBaseId)
            .ToList();

        if (items.Count == 0) return;

        items[0].Amount = newAmount;
        if (items.Count > 1)
            _context.ShoppingListItems.RemoveRange(items.Skip(1));

        _context.SaveChanges();
    }
}
