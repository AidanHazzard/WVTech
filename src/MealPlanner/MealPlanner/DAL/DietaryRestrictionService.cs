using Microsoft.EntityFrameworkCore;
using MealPlanner.Models;


namespace MealPlanner.DAL;


public class DietaryRestrictionService
{
    private readonly MealPlannerDBContext _db;
    public DietaryRestrictionService(MealPlannerDBContext db)
    {
        _db = db;
    }

    public async Task<List<DietaryRestriction>> GetAllRestrictionsAsync()
    {
        return await _db.DietaryRestrictions 
            .OrderBy(r => r.Name)
            .ToListAsync();
    }
    public async Task<List<int>> GetUserRestrictionIdsAsync(int userId)
    {
        return await _db.UserDietaryRestrictions
            .Where(x => x.UserId == userId)
            .Select(x => x.DietaryRestrictionId)
            .ToListAsync();
    }


    public async Task UpdateUserRestrictionAsync(int userId, IEnumerable<int> selectedRestrictionIds)
    {
        var selected = selectedRestrictionIds
            .Distinct()
            .ToHashSet();

        var current = await _db.UserDietaryRestrictions
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var currentIds = current.Select(x => x.DietaryRestrictionId).ToHashSet();

        var toRemove = current.Where(x => !selected.Contains(x.DietaryRestrictionId)).ToList();
        if (toRemove.Count > 0)
        {
            _db.UserDietaryRestrictions.RemoveRange(toRemove);
        }


        var toAddIds = selected.Where(id => !currentIds.Contains(id)).ToList();
        if (toAddIds.Count > 0)
        {
            var toAdd = toAddIds.Select(id => new UserDietaryRestriction
            {
                UserId = userId,
                DietaryRestrictionId = id
            });
            await _db.UserDietaryRestrictions.AddRangeAsync(toAdd);
        }
        await _db.SaveChangesAsync();
    }
}
