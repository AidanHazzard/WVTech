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
        List<Recipe>? allWithTags = null) =>
        new(
            restrictions  ?? [],
            votes         ?? [],
            percentages   ?? [],
            upvoted       ?? [],
            allWithTags   ?? []);

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

    // --- PreferredTagScorer ---

    [Test]
    public void PreferredTagScorer_NoPreferredTags_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Italian" }] };
        var ctx = EmptyContext();
        var scorer = new PreferredTagScorer([]);

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void PreferredTagScorer_AllTagsMatch_ReturnsOne()
    {
        var tag = new Tag { Id = 1, Name = "Italian" };
        var recipe = new Recipe { Id = 1, Tags = [tag] };
        var ctx = EmptyContext();
        var scorer = new PreferredTagScorer([1]);

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void PreferredTagScorer_HalfTagsMatch_ReturnsHalf()
    {
        var italian = new Tag { Id = 1, Name = "Italian" };
        var vegan   = new Tag { Id = 2, Name = "Vegan" };
        var recipe = new Recipe { Id = 1, Tags = [italian] }; // only 1 of 2 preferred tags
        var ctx = EmptyContext();
        var scorer = new PreferredTagScorer([1, 2]);

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void PreferredTagScorer_NoTagsMatch_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [new Tag { Id = 3, Name = "Mexican" }] };
        var ctx = EmptyContext();
        var scorer = new PreferredTagScorer([1, 2]);

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }

    [Test]
    public void PreferredTagScorer_RecipeWithNoTags_ReturnsZero()
    {
        var recipe = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext();
        var scorer = new PreferredTagScorer([1, 2]);

        Assert.That(scorer.Score(recipe, ctx), Is.EqualTo(0f));
    }
}
