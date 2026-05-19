using MealPlanner.Models;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class MealModelTests
{
    // --- GetMealTags ---

    [Test]
    public void GetMealTags_EmptyMeal_ReturnsEmptyList()
    {
        var meal = new Meal { Recipes = [] };

        Assert.That(meal.GetMealTags(), Is.Empty);
    }

    [Test]
    public void GetMealTags_RecipesWithNoTags_ReturnsEmptyList()
    {
        var meal = new Meal
        {
            Recipes = [new Recipe { Tags = [] }, new Recipe { Tags = [] }]
        };

        Assert.That(meal.GetMealTags(), Is.Empty);
    }

    [Test]
    public void GetMealTags_SingleRecipeWithTags_ReturnsThoseTags()
    {
        var italian = new Tag { Id = 1, Name = "Italian" };
        var vegan   = new Tag { Id = 2, Name = "Vegan" };
        var meal = new Meal { Recipes = [new Recipe { Tags = [italian, vegan] }] };

        var result = meal.GetMealTags();

        Assert.That(result.Select(t => t.Id), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void GetMealTags_MultipleRecipes_ReturnsUnionOfAllTags()
    {
        var italian  = new Tag { Id = 1, Name = "Italian" };
        var vegan    = new Tag { Id = 2, Name = "Vegan" };
        var quickMeal = new Tag { Id = 3, Name = "Quick" };
        var meal = new Meal
        {
            Recipes =
            [
                new Recipe { Tags = [italian, vegan] },
                new Recipe { Tags = [quickMeal] }
            ]
        };

        var result = meal.GetMealTags();

        Assert.That(result.Select(t => t.Id), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void GetMealTags_DuplicateTagAcrossRecipes_DeduplicatesById()
    {
        var italian = new Tag { Id = 1, Name = "Italian" };
        var meal = new Meal
        {
            Recipes =
            [
                new Recipe { Tags = [italian] },
                new Recipe { Tags = [italian] }
            ]
        };

        var result = meal.GetMealTags();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(1));
    }
}
