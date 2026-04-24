using MealPlanner.DAL.Abstract;
using MealPlanner.Models;

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
                              && i.Name.ToLower() == item.Name.ToLower()
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

    public void RemoveAllByName(string userId, string name)
    {
        var items = _context.ShoppingListItems
            .Where(i => i.UserId == userId && i.Name.ToLower() == name.ToLower())
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
            .OrderBy(i => i.Name)
            .ToList();
    }
}