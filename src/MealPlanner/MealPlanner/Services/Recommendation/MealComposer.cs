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
}
