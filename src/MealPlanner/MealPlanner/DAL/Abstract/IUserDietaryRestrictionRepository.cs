using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserDietaryRestrictionRepository
{
    Task<List<UserDietaryRestriction>> GetByUserIdAsync(string userId);
    Task SetForUserAsync(string userId, IEnumerable<int> dietaryRestrictionIds);
}
