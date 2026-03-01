using MealPlanner.Models;
using MealPlanner.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services;

public class FavoritesService : IFavoritesService
{
    private readonly MealPlannerDBContext _db;

    public FavoritesService(MealPlannerDBContext db)
    {
        _db = db;
    }

    public Task<bool> IsFavoritedAsync(string userId, int recipeId)
        => _db.UserFavoriteRecipes.AnyAsync(x => x.UserId == userId && x.RecipeId == recipeId);

    public async Task AddFavoriteAsync(string userId, int recipeId)
    {
        var exists = await IsFavoritedAsync(userId, recipeId);
        if (exists) return;

        _db.UserFavoriteRecipes.Add(new UserFavoriteRecipe { UserId = userId, RecipeId = recipeId });
        await _db.SaveChangesAsync();
    }

    public async Task RemoveFavoriteAsync(string userId, int recipeId)
    {
        var fav = await _db.UserFavoriteRecipes
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == recipeId);

        if (fav == null) return;

        _db.UserFavoriteRecipes.Remove(fav);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Recipe>> GetFavoritesAsync(string userId)
    {
        return await _db.UserFavoriteRecipes
            .Where(x => x.UserId == userId)
            .Select(x => x.Recipe)
            .ToListAsync();
    }
}