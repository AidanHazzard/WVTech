namespace MealPlanner.Services;

public static class IngredientNameNormalizer
{
    // Returns a singular form preserving original casing.
    // "potatoes" → "potato", "Carrots" → "Carrot", "asparagus" → "asparagus"
    public static string Normalize(string name)
    {
        var trimmed = name.Trim();
        var lower = trimmed.ToLower();

        if (lower.Length <= 2) return trimmed;

        // "tomatoes" → "tomato", "potatoes" → "potato" (strip "es")
        if (lower.EndsWith("oes") && lower.Length > 4)
            return trimmed[..^2];

        // don't strip 's' from asparagus, grass, virus, etc.
        if (lower.EndsWith("ss") || lower.EndsWith("us") || lower.EndsWith("is"))
            return trimmed;

        // "carrots" → "carrot", "eggs" → "egg", "cookies" → "cookie"
        if (lower.EndsWith("s") && lower.Length > 2)
            return trimmed[..^1];

        return trimmed;
    }

    // Lowercase key used for grouping and comparison.
    public static string NormalizeKey(string name) => Normalize(name).ToLower();
}
