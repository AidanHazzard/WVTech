using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Abstract;

public class UserNutritionPreferenceRepository: Repository<UserNutritionPreference>, IUserNutritionPreferenceRepository
{
    public UserNutritionPreferenceRepository(MealPlannerDBContext context):base(context)
    {}

    public async Task<UserNutritionPreference?> GetUsersNutritionPreferenceAsync(string userId)
    {
        return await _dbset.Where(p => p.UserId == userId).FirstOrDefaultAsync();
    }
}