using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

public interface IRecommendationStream
{
    Task<IEnumerable<Recipe>> GetRankedCandidatesAsync(RecommendationContext ctx);
}
