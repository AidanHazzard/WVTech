using MealPlanner.Models;

namespace MealPlanner.Services.Abstractions;

public interface IFavoritesService
{
    Task<bool> IsFavoritedAsync(string userId, int recipeId);
    Task AddFavoriteAsync(string userId, int recipeId);
    Task RemoveFavoriteAsync(string userId, int recipeId);
    Task<List<Recipe>> GetFavoritesAsync(string userId);
}