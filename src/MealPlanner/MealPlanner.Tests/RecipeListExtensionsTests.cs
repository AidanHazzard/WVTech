using MealPlanner.Helpers;
using MealPlanner.Models;
using MealPlanner.Services;
using Moq;

namespace MealPlanner.Tests;

public class RecipeListExtensionsTests
{
    private static Recipe MakeLocal(int id, string name) =>
        new Recipe { Id = id, Name = name, ExternalUri = null };

    private static Recipe MakeExternal(int id, string name, string uri) =>
        new Recipe { Id = id, Name = name, ExternalUri = uri };

    private static Recipe MakeLoaded(string name, string uri, int calories = 100) =>
        new Recipe { Name = name, ExternalUri = uri, Calories = calories, Directions = "" };

    [Test]
    public async Task LoadExternalRecipesAsync_DoesNothing_WhenServiceIsNull()
    {
        var recipes = new List<Recipe> { MakeExternal(1, "Pasta", "http://uri/1") };

        await recipes.LoadExternalRecipesAsync(null);

        Assert.That(recipes.Count, Is.EqualTo(1));
        Assert.That(recipes[0].Name, Is.EqualTo("Pasta"));
    }

    [Test]
    public async Task LoadExternalRecipesAsync_SkipsLocalRecipes()
    {
        var local = MakeLocal(1, "Local Recipe");
        var recipes = new List<Recipe> { local };

        var serviceMock = new Mock<IExternalRecipeService>();

        await recipes.LoadExternalRecipesAsync(serviceMock.Object);

        serviceMock.Verify(
            s => s.GetExternalRecipesByURIs(It.IsAny<IEnumerable<string>>()),
            Times.Never);
        Assert.That(recipes[0].Name, Is.EqualTo("Local Recipe"));
    }

    [Test]
    public async Task LoadExternalRecipesAsync_CallsBatchMethod_WithAllExternalURIs()
    {
        var recipes = new List<Recipe>
        {
            MakeExternal(1, "Pasta",   "http://uri/1"),
            MakeExternal(2, "Chicken", "http://uri/2"),
            MakeExternal(3, "Salad",   "http://uri/3"),
        };

        var serviceMock = new Mock<IExternalRecipeService>();
        serviceMock
            .Setup(s => s.GetExternalRecipesByURIs(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Recipe>
            {
                MakeLoaded("Pasta Full",   "http://uri/1"),
                MakeLoaded("Chicken Full", "http://uri/2"),
                MakeLoaded("Salad Full",   "http://uri/3"),
            });

        await recipes.LoadExternalRecipesAsync(serviceMock.Object);

        serviceMock.Verify(
            s => s.GetExternalRecipesByURIs(It.Is<IEnumerable<string>>(
                uris => uris.SequenceEqual(new[] { "http://uri/1", "http://uri/2", "http://uri/3" }))),
            Times.Once);
    }

    [Test]
    public async Task LoadExternalRecipesAsync_ReplacesRecipes_WithLoadedData()
    {
        var recipes = new List<Recipe>
        {
            MakeLocal(10,  "Local"),
            MakeExternal(20, "Pasta Stub", "http://uri/1"),
        };

        var serviceMock = new Mock<IExternalRecipeService>();
        serviceMock
            .Setup(s => s.GetExternalRecipesByURIs(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([MakeLoaded("Pasta Full", "http://uri/1", calories: 600)]);

        await recipes.LoadExternalRecipesAsync(serviceMock.Object);

        Assert.That(recipes.Count, Is.EqualTo(2));
        var pasta = recipes.First(r => r.ExternalUri == "http://uri/1");
        Assert.That(pasta.Name, Is.EqualTo("Pasta Full"));
        Assert.That(pasta.Calories, Is.EqualTo(600));
    }

    [Test]
    public async Task LoadExternalRecipesAsync_PreservesOriginalId_WhenReplacing()
    {
        var recipes = new List<Recipe> { MakeExternal(42, "Stub", "http://uri/1") };

        var serviceMock = new Mock<IExternalRecipeService>();
        serviceMock
            .Setup(s => s.GetExternalRecipesByURIs(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([MakeLoaded("Full", "http://uri/1")]);

        await recipes.LoadExternalRecipesAsync(serviceMock.Object);

        Assert.That(recipes[0].Id, Is.EqualTo(42));
    }

    [Test]
    public async Task LoadExternalRecipesAsync_RemovesRecipes_NotReturnedByService()
    {
        var recipes = new List<Recipe>
        {
            MakeExternal(1, "A", "http://uri/1"),
            MakeExternal(2, "B", "http://uri/2"),
        };

        var serviceMock = new Mock<IExternalRecipeService>();
        serviceMock
            .Setup(s => s.GetExternalRecipesByURIs(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([MakeLoaded("A Full", "http://uri/1")]);  // uri/2 missing

        await recipes.LoadExternalRecipesAsync(serviceMock.Object);

        Assert.That(recipes.Count, Is.EqualTo(1));
        Assert.That(recipes[0].ExternalUri, Is.EqualTo("http://uri/1"));
    }

    [Test]
    public async Task LoadExternalRecipesAsync_RemovesAllExternalRecipes_WhenServiceThrows()
    {
        var recipes = new List<Recipe>
        {
            MakeLocal(1,   "Local"),
            MakeExternal(2, "A", "http://uri/1"),
            MakeExternal(3, "B", "http://uri/2"),
        };

        var serviceMock = new Mock<IExternalRecipeService>();
        serviceMock
            .Setup(s => s.GetExternalRecipesByURIs(It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(new Exception("API error"));

        await recipes.LoadExternalRecipesAsync(serviceMock.Object);

        Assert.That(recipes.Count, Is.EqualTo(1));
        Assert.That(recipes[0].Name, Is.EqualTo("Local"));
    }
}
