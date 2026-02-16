using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserDietaryRestrictionRepository
{
    List<UserDietaryRestriction> GetByUserId(string userId);
    void SetForUser(string userId, List<int> dietaryRestrictionIds);
}
