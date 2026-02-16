using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class UserDietaryRestrictionRepository : IUserDietaryRestrictionRepository
{
    private readonly MealPlannerDBContext _context;

    public UserDietaryRestrictionRepository(MealPlannerDBContext context)
    {
        _context = context;
    }

    public List<UserDietaryRestriction> GetByUserId(string userId)
    {
        return _context.UserDietaryRestrictions
            .Where(x => x.UserId == userId)
            .Include(x => x.DietaryRestriction)
            .ToList();
    }

    public void SetForUser(string userId, List<int> dietaryRestrictionIds)
    {
        var existing = _context.UserDietaryRestrictions
            .Where(x => x.UserId == userId);

        _context.UserDietaryRestrictions.RemoveRange(existing);

        foreach (var restrictionId in dietaryRestrictionIds.Distinct())
        {
            _context.UserDietaryRestrictions.Add(new UserDietaryRestriction
            {
                UserId = userId,
                DietaryRestrictionId = restrictionId
            });
        }
    }
}
