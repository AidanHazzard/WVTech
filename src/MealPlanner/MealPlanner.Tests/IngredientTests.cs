using MealPlanner.Models;

namespace MealPlanner.Tests;

[TestFixture]
public class IngredientTests
{
    [Test]
    public void ToString_ReturnsStringRepresentationOfIngredient()
    {
        // Arrange
        Ingredient i = new Ingredient
        {
            DisplayName = "base",
            Amount = 0,
            IngredientBase = new IngredientBase { Name = "base" },
            Measurement = new Measurement { Name = "measure" }
        };

        // Act
        string s = i.ToString();

        // Assert
        Assert.That(s, Is.EqualTo("0 measure of base"));
    }
}
