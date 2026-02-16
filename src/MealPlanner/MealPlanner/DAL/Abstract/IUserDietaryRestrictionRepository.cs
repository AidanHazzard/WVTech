using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserDietaryRestrictionRepository
{
    Task<List<int>> GetRestrictionIdsForUserAsync(int userId);
    Task UpdateUserRestrictionAsync(int userId, IEnumerable<int> selectedRestrictionIds);
    
}