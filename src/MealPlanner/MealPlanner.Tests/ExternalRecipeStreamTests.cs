using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.Services.Recommendation;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class ExternalRecipeStreamTests
{
    private Mock<IExternalRecipeService> _externalServiceMock;
    private Mock<ITagRepository> _tagRepoMock;

    [SetUp]
    public void SetUp()
    {
        _externalServiceMock = new Mock<IExternalRecipeService>();
        _tagRepoMock = new Mock<ITagRepository>();
        _tagRepoMock
            .Setup(r => r.GetTagsByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Tag>());
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .ReturnsAsync(Enumerable.Empty<Recipe>());
    }

    private static RecommendationContext BuildContext(
        HashSet<string>? restrictions = null,
        HashSet<int>? userTags = null,
        int? calorieTarget = null,
        int? proteinTarget = null,
        int? carbTarget = null,
        int? fatTarget = null,
        HashSet<int>? mealTags = null,
        List<Recipe>? upvoted = null,
        Dictionary<int, UserVoteType>? votes = null) =>
        new(
            new UserRecommendationContext(
                restrictions ?? [],
                votes ?? [],
                [],
                upvoted ?? [],
                userTags ?? [],
                []),
            new MealRecommendationContext(
                calorieTarget,
                proteinTarget,
                carbTarget,
                fatTarget,
                mealTags ?? [],
                []));

    private ExternalRecipeStream BuildStream(
        IEnumerable<IRecipeScorer>? scorers = null,
        IEnumerable<IRecipeFilter>? filters = null,
        bool withExternal = true) =>
        new(
            _tagRepoMock.Object,
            scorers ?? [],
            filters ?? [],
            withExternal ? _externalServiceMock.Object : null);

    [Test]
    public async Task GetRankedCandidatesAsync_NullExternalService_ReturnsEmpty()
    {
        var stream = BuildStream(withExternal: false);

        var result = await stream.GetRankedCandidatesAsync(BuildContext());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRankedCandidatesAsync_NoCriteria_ReturnsEmptyWithoutCallingApi()
    {
        var stream = BuildStream();

        var result = await stream.GetRankedCandidatesAsync(BuildContext());

        Assert.That(result, Is.Empty);
        _externalServiceMock.Verify(
            s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()),
            Times.Never);
    }

    [Test]
    public async Task GetRankedCandidatesAsync_BuildsQueryFromCalorieTarget()
    {
        ExternalSearchQuery? captured = null;
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .Callback<ExternalSearchQuery>(q => captured = q)
            .ReturnsAsync([]);

        var stream = BuildStream();
        await stream.GetRankedCandidatesAsync(BuildContext(calorieTarget: 400));

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.CaloriesMin, Is.EqualTo(1));
        Assert.That(captured.CaloriesMax, Is.EqualTo(400));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_BuildsQueryFromMacroTargets()
    {
        ExternalSearchQuery? captured = null;
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .Callback<ExternalSearchQuery>(q => captured = q)
            .ReturnsAsync([]);

        var stream = BuildStream();
        await stream.GetRankedCandidatesAsync(BuildContext(proteinTarget: 50, carbTarget: 60, fatTarget: 20));

        Assert.That(captured!.ProteinMax, Is.EqualTo(50));
        Assert.That(captured.CarbsMax, Is.EqualTo(60));
        Assert.That(captured.FatMax, Is.EqualTo(20));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_LowercasesRestrictionsAsHealthFilters()
    {
        ExternalSearchQuery? captured = null;
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .Callback<ExternalSearchQuery>(q => captured = q)
            .ReturnsAsync([]);

        var stream = BuildStream();
        await stream.GetRankedCandidatesAsync(BuildContext(restrictions: ["Vegan", "Gluten-Free"]));

        Assert.That(captured!.HealthFilters, Is.EquivalentTo(new[] { "vegan", "gluten-free" }));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_LooksUpTagNamesForFreeTextQuery()
    {
        _tagRepoMock
            .Setup(r => r.GetTagsByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Tag>
            {
                new() { Id = 10, Name = "Italian" },
                new() { Id = 20, Name = "Breakfast" }
            });

        ExternalSearchQuery? captured = null;
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .Callback<ExternalSearchQuery>(q => captured = q)
            .ReturnsAsync([]);

        var stream = BuildStream();
        await stream.GetRankedCandidatesAsync(BuildContext(mealTags: [10, 20]));

        Assert.That(captured!.FreeText, Is.Not.Null);
        Assert.That(captured.FreeText, Does.Contain("Italian"));
        Assert.That(captured.FreeText, Does.Contain("Breakfast"));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_CombinesUserAndMealTagsInFreeText()
    {
        _tagRepoMock
            .Setup(r => r.GetTagsByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(10) && ids.Contains(20))))
            .ReturnsAsync(new List<Tag>
            {
                new() { Id = 10, Name = "Italian" },
                new() { Id = 20, Name = "Breakfast" }
            });

        ExternalSearchQuery? captured = null;
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .Callback<ExternalSearchQuery>(q => captured = q)
            .ReturnsAsync([]);

        var stream = BuildStream();
        await stream.GetRankedCandidatesAsync(BuildContext(userTags: [10], mealTags: [20]));

        Assert.That(captured!.FreeText, Does.Contain("Italian"));
        Assert.That(captured.FreeText, Does.Contain("Breakfast"));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_AppliesFiltersToResults()
    {
        var downvoted = new Recipe { Id = 1, Name = "Bad", Tags = [] };
        var allowed = new Recipe { Id = 2, Name = "Good", Tags = [] };
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .ReturnsAsync([downvoted, allowed]);

        var ctx = BuildContext(
            votes: new Dictionary<int, UserVoteType> { [1] = UserVoteType.DownVote },
            calorieTarget: 500);

        var stream = BuildStream(filters: [new DownVoteFilter()]);
        var result = await stream.GetRankedCandidatesAsync(ctx);

        Assert.That(result, Does.Not.Contain(downvoted));
        Assert.That(result, Does.Contain(allowed));
    }

    [Test]
    public async Task GetRankedCandidatesAsync_AppliesScorersToOrderResults()
    {
        var upvoted = new Recipe { Id = 2, Tags = [] };
        var normal  = new Recipe { Id = 1, Tags = [] };
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .ReturnsAsync([normal, upvoted]); // normal listed first

        var ctx = BuildContext(upvoted: [upvoted], calorieTarget: 500);
        var stream = BuildStream(scorers: [new UpvotePriorityScorer()]);

        var result = (await stream.GetRankedCandidatesAsync(ctx)).ToList();

        Assert.That(result[0].Id, Is.EqualTo(upvoted.Id), "Upvoted external recipe should rank first");
    }

    [Test]
    public async Task GetRankedCandidatesAsync_ApiThrows_ReturnsEmpty()
    {
        _externalServiceMock
            .Setup(s => s.SearchByContextAsync(It.IsAny<ExternalSearchQuery>()))
            .ThrowsAsync(new Exception("Edamam down"));

        var stream = BuildStream();
        var result = await stream.GetRankedCandidatesAsync(BuildContext(calorieTarget: 500));

        Assert.That(result, Is.Empty);
    }
}
