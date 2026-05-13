using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services.Recommendation;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class LocalRecipeStreamTests
{
    private static RecommendationContext EmptyContext(
        HashSet<string>? restrictions = null,
        Dictionary<int, UserVoteType>? votes = null,
        Dictionary<int, float>? percentages = null,
        List<Recipe>? upvoted = null,
        HashSet<int>? userPreferredTagIds = null,
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
                userPreferredTagIds ?? []),
            new MealRecommendationContext(
                calorieTarget,
                proteinTarget,
                carbTarget,
                fatTarget,
                mealPreferredTagIds ?? []));

    private static LocalRecipeStream BuildStream(
        List<Recipe> pool,
        IEnumerable<IRecipeScorer>? scorers = null,
        IEnumerable<IRecipeFilter>? filters = null)
    {
        var repoMock = new Mock<IRecipeRepository>();
        repoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync(pool);
        return new LocalRecipeStream(
            repoMock.Object,
            scorers ?? [
                new UpvotePriorityScorer(),
                new VotePercentageScorer(),
                new UserPreferredTagScorer(),
                new MealPreferredTagScorer()
            ],
            filters ?? [
                new DownVoteFilter(),
                new DietaryRestrictionFilter(),
                new PreferredTagFilter()
            ]);
    }

    [Test]
    public async Task GetRankedCandidatesAsync_EmptyPool_ReturnsEmpty()
    {
        var stream = BuildStream([]);

        var result = await stream.GetRankedCandidatesAsync(EmptyContext());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRankedCandidatesAsync_ReturnsAllUnfilteredRecipes()
    {
        var a = new Recipe { Id = 1, Tags = [] };
        var b = new Recipe { Id = 2, Tags = [] };
        var stream = BuildStream([a, b]);

        var result = (await stream.GetRankedCandidatesAsync(EmptyContext())).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_ExcludesDownvotedRecipes()
    {
        var downvoted = new Recipe { Id = 1, Tags = [] };
        var allowed   = new Recipe { Id = 2, Tags = [] };
        var ctx = EmptyContext(votes: new Dictionary<int, UserVoteType> { [1] = UserVoteType.DownVote });
        var stream = BuildStream([downvoted, allowed]);

        var result = await stream.GetRankedCandidatesAsync(ctx);

        Assert.That(result, Does.Not.Contain(downvoted));
        Assert.That(result, Does.Contain(allowed));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_ExcludesDietaryRestrictionMismatch()
    {
        var vegan    = new Recipe { Id = 1, Tags = [new Tag { Id = 1, Name = "Vegan" }] };
        var nonVegan = new Recipe { Id = 2, Tags = [] };
        var ctx = EmptyContext(restrictions: ["Vegan"]);
        var stream = BuildStream([vegan, nonVegan]);

        var result = await stream.GetRankedCandidatesAsync(ctx);

        Assert.That(result, Does.Contain(vegan));
        Assert.That(result, Does.Not.Contain(nonVegan));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_PutsUpvotedRecipeFirst()
    {
        var upvoted = new Recipe { Id = 2, Tags = [] };
        var normal  = new Recipe { Id = 1, Tags = [] };
        var ctx = EmptyContext(upvoted: [upvoted]);
        var stream = BuildStream([normal, upvoted]); // normal listed first in pool

        var result = (await stream.GetRankedCandidatesAsync(ctx)).ToList();

        Assert.That(result[0].Id, Is.EqualTo(upvoted.Id));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_OrdersByVotePercentageAmongNonUpvoted()
    {
        var highVote = new Recipe { Id = 1, Tags = [] };
        var lowVote  = new Recipe { Id = 2, Tags = [] };
        var ctx = EmptyContext(percentages: new Dictionary<int, float> { [1] = 0.8f, [2] = 0.2f });
        var stream = BuildStream([lowVote, highVote]); // lowVote listed first in pool

        var result = (await stream.GetRankedCandidatesAsync(ctx)).ToList();

        Assert.That(result[0].Id, Is.EqualTo(highVote.Id));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_PreferredTagFilter_RejectsNonMatchingWhenSlotHasTags()
    {
        var matching    = new Recipe { Id = 1, Tags = [new Tag { Id = 10, Name = "Breakfast" }] };
        var nonMatching = new Recipe { Id = 2, Tags = [new Tag { Id = 20, Name = "Dessert" }] };
        var ctx = EmptyContext(mealPreferredTagIds: [10]);
        var stream = BuildStream([matching, nonMatching]);

        var result = await stream.GetRankedCandidatesAsync(ctx);

        Assert.That(result, Does.Contain(matching));
        Assert.That(result, Does.Not.Contain(nonMatching));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_OrdersByMealPreferredTagMatches()
    {
        var doubleMatch = new Recipe { Id = 1, Tags = [new Tag { Id = 10 }, new Tag { Id = 11 }] };
        var singleMatch = new Recipe { Id = 2, Tags = [new Tag { Id = 10 }] };
        var ctx = EmptyContext(mealPreferredTagIds: [10, 11]);
        var stream = BuildStream([singleMatch, doubleMatch]); // singleMatch listed first

        var result = (await stream.GetRankedCandidatesAsync(ctx)).ToList();

        Assert.That(result[0].Id, Is.EqualTo(doubleMatch.Id));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_EmptyScorersAndFilters_DoesNotThrow()
    {
        var repoMock = new Mock<IRecipeRepository>();
        repoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([]);
        var stream = new LocalRecipeStream(repoMock.Object, [], []);

        Assert.DoesNotThrowAsync(() => stream.GetRankedCandidatesAsync(EmptyContext()));
    }
}
