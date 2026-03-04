using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserRepository : IRepository<User>
{
    public Task<List<Recipe>> GetUserOwnedRecipesByUserIdAsync(string id);
}