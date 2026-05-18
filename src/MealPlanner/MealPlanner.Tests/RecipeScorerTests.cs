using MealPlanner.Models;
using MealPlanner.Services.Recommendation;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class RecipeScorerTests
{
    private static RecommendationContext EmptyContext(
        HashSet<string>? restrictions = null,
        Dictionary<int, UserVoteType>? votes = null,
        Dictionary<int, float>? percentages = null,
        List<Recipe>? upvoted = null,
        HashSet<int>? userPreferredTagIds = null,
        HashSet<string>? pantryIngredientNames = null,
        int? calorieTarget = null,
        int? proteinTarget = null,
        int? carbTarget = null,
        int? fatTarget = null,
        HashSet<int>? mealPreferredTagIds = null) =>
        new(
            new UserRecommendationContext(
                restrictions ?? [],
                votes ?? [],
                percentages ?? [],
                upvoted ?? [],
                userPreferredTagIds ?? [],
                pantryIngredientNames ?? []),
            new MealRecommendationContext(
                calorieTarget,
                proteinTarget,
                carbTarget,
                fatTarget,
                mealPreferredTagIds ?? []));

    // --- UpvotePriorityScorer ---

    [Test]
    public void UpvotePriorityScorer_UpvotedRecipe_ReturnsPositiveScore()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(upvoted: [recipe]);
        var scorer = new UpvotePriorityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.GreaterThan(0f));
    }

    [Test]
    public void UpvotePriorityScorer_NonUpvotedRecipe_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(upvoted: []);
        var scorer = new UpvotePriorityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void UpvotePriorityScorer_UpvotedScoreExceedsMaxVotePercentage()
    {
        // Upvote score must dominate vote%, so it must be > 1.0 (the max normalized vote%).
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(upvoted: [recipe]);
        var scorer = new UpvotePriorityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.GreaterThan(1f));
    }

    // --- VotePercentageScorer ---

    [Test]
    public void VotePercentageScorer_ReturnsNormalizedVotePercentage()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(percentages: new Dictionary<int, float> { [1] = 0.75f });
        var scorer = new VotePercentageScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0.75f).Within(0.001f));
    }

    [Test]
    public void VotePercentageScorer_RecipeNotInDictionary_ReturnsZero()
    {
        var recipe = new Recipe { Id = 99, Tags = [] };
        var ctx = EmptyContext(percentages: []);
        var scorer = new VotePercentageScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    // --- DownVoteFilter ---

    [Test]
    public void DownVoteFilter_DownvotedRecipe_ReturnsFalse()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(votes: new Dictionary<int, UserVoteType> { [1] = UserVoteType.DownVote });
        var filter = new DownVoteFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.False);
    }

    [Test]
    public void DownVoteFilter_NoVoteRecipe_ReturnsTrue()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(votes: new Dictionary<int, UserVoteType> { [1] = UserVoteType.NoVote });
        var filter = new DownVoteFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    [Test]
    public void DownVoteFilter_UpvotedRecipe_ReturnsTrue()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(votes: new Dictionary<int, UserVoteType> { [1] = UserVoteType.UpVote });
        var filter = new DownVoteFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    [Test]
    public void DownVoteFilter_RecipeNotInDictionary_ReturnsTrue()
    {
        var recipe = new Recipe { Id = 99, Tags = [] };
        var ctx = EmptyContext(votes: []);
        var filter = new DownVoteFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    // --- MealPreferredTagScorer ---

    [Test]
    public void MealPreferredTagScorer_NoPreferredTags_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(mealPreferredTagIds: []);
        var scorer = new MealPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void MealPreferredTagScorer_AllTagsMatch_ReturnsOne()
    {
        var tag = new Tag { Id = 1, Name = "Italian" };
        var recipe = new Recipe { Id = 1, Tags = [tag] };
        var ctx = EmptyContext(mealPreferredTagIds: [1]);
        var scorer = new MealPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void MealPreferredTagScorer_HalfTagsMatch_ReturnsHalf()
    {
        var italian = new Tag { Id = 1, Name = "Italian" };
        var recipe = new Recipe { Id = 1, Tags = [italian] }; // only 1 of 2 preferred tags
        var ctx = EmptyContext(mealPreferredTagIds: [1, 2]);
        var scorer = new MealPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void MealPreferredTagScorer_NoTagsMatch_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 3, Name = "Mexican" }] };
        var ctx = EmptyContext(mealPreferredTagIds: [1, 2]);
        var scorer = new MealPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void MealPreferredTagScorer_RecipeWithNoTags_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(mealPreferredTagIds: [1, 2]);
        var scorer = new MealPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void MealPreferredTagScorer_IgnoresUserLevelPreferredTags()
    {
        // Recipe matches a user-level pref, but the meal slot has no preference for it.
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(userPreferredTagIds: [1], mealPreferredTagIds: []);
        var scorer = new MealPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    // --- UserPreferredTagScorer ---

    [Test]
    public void UserPreferredTagScorer_NoPreferredTags_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(userPreferredTagIds: []);
        var scorer = new UserPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void UserPreferredTagScorer_AllTagsMatch_ReturnsOne()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(userPreferredTagIds: [1]);
        var scorer = new UserPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void UserPreferredTagScorer_HalfTagsMatch_ReturnsHalf()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(userPreferredTagIds: [1, 2]);
        var scorer = new UserPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void UserPreferredTagScorer_IgnoresMealLevelPreferredTags()
    {
        // Recipe matches a slot-level pref, but the user has no standing prefs.
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(userPreferredTagIds: [], mealPreferredTagIds: [1]);
        var scorer = new UserPreferredTagScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    // --- PreferredTagFilter ---

    [Test]
    public void PreferredTagFilter_NoMealPreferredTags_AllowsAnyRecipe()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(mealPreferredTagIds: []);
        var filter = new PreferredTagFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    [Test]
    public void PreferredTagFilter_RecipeMatchesAtLeastOneTag_Allows()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(mealPreferredTagIds: [1, 2]);
        var filter = new PreferredTagFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    [Test]
    public void PreferredTagFilter_RecipeMatchesNoTags_Rejects()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 3, Name = "Mexican" }] };
        var ctx = EmptyContext(mealPreferredTagIds: [1, 2]);
        var filter = new PreferredTagFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.False);
    }

    [Test]
    public void PreferredTagFilter_RecipeWithNoTagsAndSlotHasPreference_Rejects()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(mealPreferredTagIds: [1]);
        var filter = new PreferredTagFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.False);
    }

    [Test]
    public void PreferredTagFilter_IgnoresUserLevelPreferredTags()
    {
        // Recipe doesn't match the slot pref but does match a user pref: still rejected.
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(userPreferredTagIds: [1], mealPreferredTagIds: [2]);
        var filter = new PreferredTagFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.False);
    }

    // --- DietaryRestrictionFilter ---

    [Test]
    public void DietaryRestrictionFilter_NoRestrictions_AllowsAnyRecipe()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(restrictions: []);
        var filter = new DietaryRestrictionFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    [Test]
    public void DietaryRestrictionFilter_RecipeHasMatchingTag_ReturnsTrue()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Vegan" }] };
        var ctx = EmptyContext(restrictions: ["Vegan"]);
        var filter = new DietaryRestrictionFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    [Test]
    public void DietaryRestrictionFilter_RecipeMissingRequiredTag_ReturnsFalse()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(restrictions: ["Vegan"]);
        var filter = new DietaryRestrictionFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.False);
    }

    [Test]
    public void DietaryRestrictionFilter_MultipleRestrictions_RequiresAllTagsPresent()
    {
        var veganOnly = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Vegan" }] };
        var ctx = EmptyContext(restrictions: ["Vegan", "Gluten-Free"]);
        var filter = new DietaryRestrictionFilter();

        Assert.That(filter.Allow(veganOnly, ctx), Is.False);
    }

    [Test]
    public void DietaryRestrictionFilter_AllRestrictionsPresent_ReturnsTrue()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Tags = [new Tag { Id = 1, Name = "Vegan" }, new Tag { Id = 2, Name = "Gluten-Free" }]
        };
        var ctx = EmptyContext(restrictions: ["Vegan", "Gluten-Free"]);
        var filter = new DietaryRestrictionFilter();

        Assert.That(filter.Allow(recipe, ctx), Is.True);
    }

    // --- TagSimilarityScorer ---

    [Test]
    public void TagSimilarityScorer_NoUpvotedRecipes_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(upvoted: []);
        var scorer = new TagSimilarityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void TagSimilarityScorer_UpvotedRecipesHaveNoTags_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext(upvoted: [new Recipe { Id = 2, Tags = [] }]);
        var scorer = new TagSimilarityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void TagSimilarityScorer_RecipeMatchesEntireProfile_ReturnsOne()
    {
        var ctx = EmptyContext(upvoted: [new Recipe { Id = 2, Tags = [new Tag { Id = 1, Name = "Italian" }] }]);
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var scorer = new TagSimilarityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void TagSimilarityScorer_RecipeMatchesNoProfileTags_ReturnsZero()
    {
        var ctx = EmptyContext(upvoted: [new Recipe { Id = 2, Tags = [new Tag { Id = 1, Name = "Italian" }] }]);
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 99, Name = "Mexican" }] };
        var scorer = new TagSimilarityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void TagSimilarityScorer_RecipeWithNoTags_ReturnsZero()
    {
        var ctx = EmptyContext(upvoted: [new Recipe { Id = 2, Tags = [new Tag { Id = 1, Name = "Italian" }] }]);
        var recipe = new Recipe { Id = 1, Tags = [] };
        var scorer = new TagSimilarityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void TagSimilarityScorer_PartialProfileOverlap_ReturnsFraction()
    {
        // Profile built from one upvoted recipe carrying two equally-weighted tags.
        var ctx = EmptyContext(upvoted:
        [
            new Recipe { Id = 2, Tags = [new Tag { Id = 1 }, new Tag { Id = 2 }] }
        ]);
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1 }] };
        var scorer = new TagSimilarityScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void TagSimilarityScorer_FrequentTagOutweighsRareTag()
    {
        // Tag 1 appears in all three upvoted recipes; tag 2 appears in only one.
        var ctx = EmptyContext(upvoted:
        [
            new Recipe { Id = 2, Tags = [new Tag { Id = 1 }] },
            new Recipe { Id = 3, Tags = [new Tag { Id = 1 }] },
            new Recipe { Id = 4, Tags = [new Tag { Id = 1 }, new Tag { Id = 2 }] }
        ]);
        var scorer = new TagSimilarityScorer();
        var frequentTagRecipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1 }] };
        var rareTagRecipe = new Recipe { Id = 5, Tags = [new Tag { Id = 2 }] };

        Assert.That(
            scorer.Score(frequentTagRecipe, ctx),
            Is.GreaterThan(scorer.Score(rareTagRecipe, ctx)));
    }

    // --- NutrientFitScorer ---

    [Test]
    public void NutrientFitScorer_NoMacroTargets_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [], Protein = 30, Carbs = 40, Fat = 10 };
        var ctx = EmptyContext();
        var scorer = new NutrientFitScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void NutrientFitScorer_PartialMacroTargets_ReturnsZero()
    {
        // Only protein target set — composition needs all three macros.
        var recipe = new Recipe { Id = 1, Tags = [], Protein = 30, Carbs = 40, Fat = 10 };
        var ctx = EmptyContext(proteinTarget: 50);
        var scorer = new NutrientFitScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void NutrientFitScorer_RecipeWithNoMacros_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [], Protein = 0, Carbs = 0, Fat = 0 };
        var ctx = EmptyContext(proteinTarget: 30, carbTarget: 40, fatTarget: 10);
        var scorer = new NutrientFitScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void NutrientFitScorer_RecipeMatchesTargetComposition_ReturnsOne()
    {
        var recipe = new Recipe { Id = 1, Tags = [], Protein = 30, Carbs = 40, Fat = 10 };
        var ctx = EmptyContext(proteinTarget: 30, carbTarget: 40, fatTarget: 10);
        var scorer = new NutrientFitScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void NutrientFitScorer_SameCompositionDifferentScale_ReturnsOne()
    {
        // Recipe carries double the target's macros but the same balance.
        var recipe = new Recipe { Id = 1, Tags = [], Protein = 60, Carbs = 80, Fat = 20 };
        var ctx = EmptyContext(proteinTarget: 30, carbTarget: 40, fatTarget: 10);
        var scorer = new NutrientFitScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void NutrientFitScorer_OppositeComposition_ScoresLow()
    {
        // Target is almost all protein; recipe is almost all fat.
        var recipe = new Recipe { Id = 1, Tags = [], Protein = 5, Carbs = 5, Fat = 90 };
        var ctx = EmptyContext(proteinTarget: 90, carbTarget: 5, fatTarget: 5);
        var scorer = new NutrientFitScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.LessThan(0.3f));
    }

    [Test]
    public void NutrientFitScorer_CloserCompositionScoresHigher()
    {
        var ctx = EmptyContext(proteinTarget: 30, carbTarget: 40, fatTarget: 10);
        var scorer = new NutrientFitScorer();
        var near = new Recipe { Id = 1, Tags = [], Protein = 28, Carbs = 42, Fat = 10 };
        var far = new Recipe { Id = 2, Tags = [], Protein = 5, Carbs = 5, Fat = 90 };

        Assert.That(scorer.Score(near, ctx), Is.GreaterThan(scorer.Score(far, ctx)));
    }

    // --- PantryOverlapScorer ---

    private static Recipe RecipeWithIngredients(int id, params string[] ingredientNames) =>
        new()
        {
            Id = id,
            Tags = [],
            Ingredients = ingredientNames
                .Select(n => new Ingredient
                {
                    DisplayName = n,
                    IngredientBase = new IngredientBase { Name = n },
                    Measurement = new Measurement { Name = "unit" }
                })
                .ToList()
        };

    [Test]
    public void PantryOverlapScorer_EmptyPantry_ReturnsZero()
    {
        var recipe = RecipeWithIngredients(1, "egg", "flour");
        var ctx = EmptyContext(pantryIngredientNames: []);
        var scorer = new PantryOverlapScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void PantryOverlapScorer_RecipeWithNoIngredients_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [], Ingredients = [] };
        var ctx = EmptyContext(pantryIngredientNames: ["egg"]);
        var scorer = new PantryOverlapScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void PantryOverlapScorer_AllIngredientsInPantry_ReturnsOne()
    {
        var recipe = RecipeWithIngredients(1, "egg", "flour");
        var ctx = EmptyContext(pantryIngredientNames: ["egg", "flour"]);
        var scorer = new PantryOverlapScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void PantryOverlapScorer_NoIngredientsInPantry_ReturnsZero()
    {
        var recipe = RecipeWithIngredients(1, "egg", "flour");
        var ctx = EmptyContext(pantryIngredientNames: ["beef", "rice"]);
        var scorer = new PantryOverlapScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void PantryOverlapScorer_HalfIngredientsInPantry_ReturnsHalf()
    {
        var recipe = RecipeWithIngredients(1, "egg", "flour");
        var ctx = EmptyContext(pantryIngredientNames: ["egg"]);
        var scorer = new PantryOverlapScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void PantryOverlapScorer_NormalizesIngredientNamesBeforeMatching()
    {
        // Recipe lists plural "Eggs"/"Tomatoes"; the pantry holds the
        // singular, lowercased keys the normalizer produces.
        var recipe = RecipeWithIngredients(1, "Eggs", "Tomatoes");
        var ctx = EmptyContext(pantryIngredientNames: ["egg", "tomato"]);
        var scorer = new PantryOverlapScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void PantryOverlapScorer_CountsDistinctIngredientsOnce()
    {
        // "egg" is listed twice but counts as a single ingredient, so one of
        // two distinct ingredients matching the pantry scores 0.5.
        var recipe = RecipeWithIngredients(1, "egg", "egg", "flour");
        var ctx = EmptyContext(pantryIngredientNames: ["egg"]);
        var scorer = new PantryOverlapScorer();

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0.5f).Within(0.001f));
    }
}
