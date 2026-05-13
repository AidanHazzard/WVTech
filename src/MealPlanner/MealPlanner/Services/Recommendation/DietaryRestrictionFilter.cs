using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

public sealed class DietaryRestrictionFilter : IRecipeFilter
{
    public bool Allow(Recipe recipe, RecommendationContext ctx) =>
        ctx.RestrictionNames.Count == 0
        || ctx.RestrictionNames.All(name => recipe.Tags.Any(t => t.Name == name));
}
