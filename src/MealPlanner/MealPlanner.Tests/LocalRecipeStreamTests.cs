using MealPlanner.DAL.Abstract;
using MealPlanner.Models;
using MealPlanner.Services.Recommendation;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests;

[TestFixture]
public class LocalRecipeStreamTests
{
    [Test]
    public async Task GetCandidatesAsync_EmptyRepository_ReturnsEmpty()
    {
        var repoMock = new Mock<IRecipeRepository>();
        repoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([]);
        var stream = new LocalRecipeStream(repoMock.Object);

        var result = await stream.GetCandidatesAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetCandidatesAsync_ReturnsRecipesFromRepository()
    {
        var recipe = new Recipe { Id = 1, Name = "Pasta", Tags = [] };
        var repoMock = new Mock<IRecipeRepository>();
        repoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([recipe]);
        var stream = new LocalRecipeStream(repoMock.Object);

        var result = await stream.GetCandidatesAsync();

        Assert.That(result, Does.Contain(recipe));
    }

    [Test]
    public async Task GetCandidatesAsync_NullExternalService_DoesNotThrow()
    {
        var repoMock = new Mock<IRecipeRepository>();
        repoMock.Setup(r => r.GetAllWithTagsAsync()).ReturnsAsync([]);
        var stream = new LocalRecipeStream(repoMock.Object, externalRecipeService: null);

        Assert.DoesNotThrowAsync(() => stream.GetCandidatesAsync());
        await stream.GetCandidatesAsync();
    }
}
