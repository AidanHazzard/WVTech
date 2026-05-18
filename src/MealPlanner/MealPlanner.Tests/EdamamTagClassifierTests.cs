using MealPlanner.Services;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class EdamamTagClassifierTests
{
    [Test]
    public void Classify_CuisineTag_RoutesToCuisineTypes()
    {
        var result = EdamamTagClassifier.Classify(["Italian"]);
        Assert.That(result.CuisineTypes, Does.Contain("Italian"));
    }

    [Test]
    public void Classify_MealTypeTag_RoutesToMealTypes()
    {
        var result = EdamamTagClassifier.Classify(["Breakfast"]);
        Assert.That(result.MealTypes, Does.Contain("Breakfast"));
    }

    [Test]
    public void Classify_DishTypeTag_RoutesToDishTypes()
    {
        var result = EdamamTagClassifier.Classify(["Salad"]);
        Assert.That(result.DishTypes, Does.Contain("Salad"));
    }

    [Test]
    public void Classify_DietTag_RoutesToDiets()
    {
        var result = EdamamTagClassifier.Classify(["high-protein"]);
        Assert.That(result.Diets, Does.Contain("high-protein"));
    }

    [Test]
    public void Classify_HealthTag_RoutesToHealthLabels()
    {
        var result = EdamamTagClassifier.Classify(["Vegan"]);
        Assert.That(result.HealthLabels, Does.Contain("vegan"));
    }

    [Test]
    public void Classify_UnknownTag_RoutesToFreeText()
    {
        var result = EdamamTagClassifier.Classify(["Comfort Food"]);
        Assert.That(result.FreeTextTerms, Does.Contain("Comfort Food"));
    }

    [Test]
    public void Classify_MatchingIsCaseInsensitive()
    {
        var result = EdamamTagClassifier.Classify(["italian"]);
        Assert.That(result.CuisineTypes, Does.Contain("Italian"));
    }

    [Test]
    public void Classify_MatchingIsSeparatorInsensitive()
    {
        // Our tags use spaces; Edamam's diet values are hyphenated.
        var result = EdamamTagClassifier.Classify(["Low Carb", "Middle Eastern"]);
        Assert.That(result.Diets, Does.Contain("low-carb"));
        Assert.That(result.CuisineTypes, Does.Contain("Middle Eastern"));
    }

    [Test]
    public void Classify_AppliesDishTypeAlias()
    {
        // The likely tag "Dessert" maps to Edamam's "Desserts".
        var result = EdamamTagClassifier.Classify(["Dessert"]);
        Assert.That(result.DishTypes, Does.Contain("Desserts"));
    }

    [Test]
    public void Classify_MixedTags_RoutesEachToItsOwnFacet()
    {
        var result = EdamamTagClassifier.Classify(["Italian", "Breakfast", "Comfort Food"]);
        Assert.That(result.CuisineTypes, Does.Contain("Italian"));
        Assert.That(result.MealTypes, Does.Contain("Breakfast"));
        Assert.That(result.FreeTextTerms, Does.Contain("Comfort Food"));
    }

    [Test]
    public void Classify_DeduplicatesRepeatedFacetValues()
    {
        var result = EdamamTagClassifier.Classify(["Italian", "italian"]);
        Assert.That(result.CuisineTypes.Count, Is.EqualTo(1));
    }

    [Test]
    public void Classify_EmptyInput_ReturnsEmptySelection()
    {
        var result = EdamamTagClassifier.Classify([]);
        Assert.That(result.CuisineTypes, Is.Empty);
        Assert.That(result.FreeTextTerms, Is.Empty);
    }
}
