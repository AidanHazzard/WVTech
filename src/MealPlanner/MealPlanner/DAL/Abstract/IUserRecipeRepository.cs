using MealPlanner.Models;

namespace MealPlanner.DAL.Abstract;

public interface IUserRecipeRepository : IRepository<UserRecipe>
{
    public Task<List<Recipe>> GetUserOwnedRecipesByUserIdAsync(string id);
    public Task<float> GetRecipeVotePercentage(int id);

    public Task<List<Recipe>> GetFavoritesAsync(string id);

    public Task AddFavoriteAsync(User user, Recipe recipe);

    public Task RemoveFavoriteAsync(string userId, int recipeId);

    public Task<bool> IsUserRecipeOwnerAsync(string userId, int recipeId);

    public Task ChangeRecipeVoteAsync(User user, Recipe recipe, UserVoteType voteType);
    
    public Task<UserVoteType> GetUserRecipeVoteAsync(string userId, int recipeId);
}