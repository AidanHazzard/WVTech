using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.DAL.Concrete;

public class UserRecipeRepository :  Repository<UserRecipe>, IUserRecipeRepository
{
    public UserRecipeRepository(MealPlannerDBContext context) : base(context)
    {}
    
    public async Task<List<Recipe>> GetUserOwnedRecipesByUserIdAsync(string id)
    {
        return await _dbset
            .Where(ur => ur.UserId == id && ur.UserOwner)
            .Include(ur => ur.Recipe)
            .Select(ur => ur.Recipe)
            .ToListAsync();
    }

    public async Task<bool> IsUserRecipeOwner(string userId, int recipeId)
    {
        var ur = await _dbset.FindAsync(userId, recipeId);
        return ur?.UserOwner ?? false;
    }

    public async Task<List<Recipe>> GetFavoritesAsync(string userId)
    {
        return await _dbset
            .Where(ur => ur.UserId == userId && ur.UserFavorite)
            .Include(ur => ur.Recipe)
            .Select(ur => ur.Recipe)
            .ToListAsync();
    }

    public async Task AddFavoriteAsync(User user, Recipe recipe)
    {
        UserRecipe userRecipe = 
            await _dbset.FindAsync(user.Id, recipe.Id)
            ?? new UserRecipe 
            { 
                User = user, 
                Recipe = recipe
            };
        
        userRecipe.UserFavorite = true;
        CreateOrUpdate(userRecipe);
    }

    public async Task RemoveFavoriteAsync(string userId, int recipeId)
    {
        UserRecipe? userRecipe = await _dbset.FindAsync(userId, recipeId);

        if (userRecipe == null ) return;

        userRecipe.UserFavorite = false;

        if (userRecipe.Redundant())
        {
            Delete(userRecipe);
        }
        else
        {
            CreateOrUpdate(userRecipe);
        }
    }

    public async Task<float> GetRecipeVotePercentage(int id)
    {
        int totalVotes = 
            await _dbset
            .Where(ur => ur.RecipeId == id && ur.UserVote != UserVoteType.NoVote)
            .CountAsync();
        
        if (totalVotes == 0) return 0f;
        int upVotes =
            await _dbset
            .Where(ur => ur.RecipeId == id && ur.UserVote == UserVoteType.UpVote)
            .CountAsync();
        
        return upVotes / (float) totalVotes;
    }
}