namespace MealPlanner.Services;

/// <summary>
/// Routes recipe tag names onto Edamam recipes/v2 search facets — diet,
/// health, cuisineType, mealType, dishType. Matching is case- and
/// separator-insensitive ("Low Carb" matches "low-carb"), with a small alias
/// map for tag names that differ from Edamam's wording. A tag matching no
/// facet becomes a free-text term; a tag matching several is assigned by
/// precedence: diet, health, cuisine, meal, dish.
/// </summary>
public static class EdamamTagClassifier
{
    public static EdamamFacetSelection Classify(IEnumerable<string> tagNames) =>
        new([], [], [], [], [],
            tagNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToList());
}
