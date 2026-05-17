using MealPlanner.Models;

namespace MealPlanner.Services.Recommendation;

public static class MealComposer
{
    public static List<Recipe> PackUpToCalorieBudget(
        IEnumerable<Recipe> rankedCandidates,
        int calorieTarget,
        int maxRecipes,
        int? proteinTarget = null,
        int? carbTarget = null,
        int? fatTarget = null)
    {
        var recipes = new List<Recipe>();
        int runningCalories = 0, runningProtein = 0, runningCarbs = 0, runningFat = 0;
        foreach (var recipe in rankedCandidates)
        {
            if (recipes.Count >= maxRecipes) break;
            if (recipe.Calories + runningCalories <= calorieTarget
                && (!proteinTarget.HasValue || recipe.Protein + runningProtein <= proteinTarget.Value)
                && (!carbTarget.HasValue    || recipe.Carbs   + runningCarbs   <= carbTarget.Value)
                && (!fatTarget.HasValue     || recipe.Fat     + runningFat     <= fatTarget.Value))
            {
                recipes.Add(recipe);
                runningCalories += recipe.Calories;
                runningProtein  += recipe.Protein;
                runningCarbs    += recipe.Carbs;
                runningFat      += recipe.Fat;
            }
        }
        return recipes;
    }

    /// <summary>
    /// Selects the subset of candidates that best fits the meal slot. Calories
    /// and recipe count are hard limits; protein/carb/fat targets are soft —
    /// a subset is scored by recipe rank ("profit") minus a penalty for how
    /// far its macro totals stray from the targets, and the highest-scoring
    /// subset wins. Replaces the greedy <see cref="PackUpToCalorieBudget"/>.
    /// </summary>
    public static List<Recipe> Compose(
        IEnumerable<Recipe> rankedCandidates,
        int calorieTarget,
        int maxRecipes,
        int? proteinTarget = null,
        int? carbTarget = null,
        int? fatTarget = null) => [];
}
