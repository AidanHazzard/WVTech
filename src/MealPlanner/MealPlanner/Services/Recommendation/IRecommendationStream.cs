using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

public interface IRecommendationStream
{
    Task<List<Recipe>> GetCandidatesAsync();
}
