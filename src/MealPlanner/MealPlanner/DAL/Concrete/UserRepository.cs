using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.DAL.Concrete;

public class UserRepository :  Repository<User>, IUserRepository
{
    public UserRepository(MealPlannerDBContext context) : base(context)
    {}
    
    public Task<List<Recipe>> GetUserOwnedRecipesByUserIdAsync(string id)
    {
        return _dbset.Include(u => u.OwnedRecipes)
                    .Where(u => u.Id == id)
                    .Select(u => u.OwnedRecipes)
                    .FirstAsync();
    }
}