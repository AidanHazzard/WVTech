using MealPlanner.Models;
using MealPlanner.Services.Recommendation;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class MealComposerTests
{
    private static Recipe R(int id, int calories, int protein = 0, int carbs = 0, int fat = 0) =>
        new() { Id = id, Name = $"R{id}", Calories = calories, Protein = protein, Carbs = carbs, Fat = fat, Tags = [] };

    [Test]
    public void PackUpToCalorieBudget_EmptyInput_ReturnsEmpty()
    {
        var result = MealComposer.PackUpToCalorieBudget([], calorieTarget: 2000, maxRecipes: 5);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void PackUpToCalorieBudget_RecipeWithinBudget_IsIncluded()
    {
        var recipe = R(1, 300);

        var result = MealComposer.PackUpToCalorieBudget([recipe], calorieTarget: 500, maxRecipes: 5);

        Assert.That(result, Does.Contain(recipe));
    }

    [Test]
    public void PackUpToCalorieBudget_RecipeExceedingBudget_IsExcluded()
    {
        var recipe = R(1, 600);

        var result = MealComposer.PackUpToCalorieBudget([recipe], calorieTarget: 500, maxRecipes: 5);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void PackUpToCalorieBudget_RunningTotalBlocksSecondRecipe()
    {
        var a = R(1, 300);
        var b = R(2, 300);

        var result = MealComposer.PackUpToCalorieBudget([a, b], calorieTarget: 400, maxRecipes: 5);

        Assert.That(result, Does.Contain(a));
        Assert.That(result, Does.Not.Contain(b));
    }

    [Test]
    public void PackUpToCalorieBudget_MaxRecipesLimit_IsEnforced()
    {
        var recipes = Enumerable.Range(1, 10).Select(i => R(i, 50)).ToList();

        var result = MealComposer.PackUpToCalorieBudget(recipes, calorieTarget: 9999, maxRecipes: 3);

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void PackUpToCalorieBudget_ProteinBudgetExceeded_RecipeExcluded()
    {
        var recipe = R(1, calories: 100, protein: 50);

        var result = MealComposer.PackUpToCalorieBudget([recipe], calorieTarget: 9999, maxRecipes: 5, proteinTarget: 20);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void PackUpToCalorieBudget_NullProteinTarget_HighProteinRecipeAllowed()
    {
        var recipe = R(1, calories: 100, protein: 999);

        var result = MealComposer.PackUpToCalorieBudget([recipe], calorieTarget: 9999, maxRecipes: 5, proteinTarget: null);

        Assert.That(result, Does.Contain(recipe));
    }

    [Test]
    public void PackUpToCalorieBudget_CarbAndFatBudgets_AreCheckedIndependently()
    {
        var failsCarbs = R(1, calories: 10, carbs: 50);
        var failsFat   = R(2, calories: 10, fat: 50);
        var fits       = R(3, calories: 10, carbs: 5, fat: 5);

        var result = MealComposer.PackUpToCalorieBudget(
            [failsCarbs, failsFat, fits],
            calorieTarget: 9999, maxRecipes: 5,
            carbTarget: 10, fatTarget: 10);

        Assert.That(result, Does.Not.Contain(failsCarbs));
        Assert.That(result, Does.Not.Contain(failsFat));
        Assert.That(result, Does.Contain(fits));
    }

    [Test]
    public void PackUpToCalorieBudget_PreservesInputOrder()
    {
        var a = R(1, 100);
        var b = R(2, 100);
        var c = R(3, 100);

        var result = MealComposer.PackUpToCalorieBudget([c, b, a], calorieTarget: 9999, maxRecipes: 5);

        Assert.That(result.Select(r => r.Id), Is.EqualTo(new[] { 3, 2, 1 }));
    }
}
