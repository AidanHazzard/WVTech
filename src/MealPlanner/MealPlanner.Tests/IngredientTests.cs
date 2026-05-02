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

[TestFixture]
public class IngredientBaseTests
{
    [Test]
    public void Display_CapitalizesFirstLetter()
    {
        var ib = new IngredientBase { Name = "chicken breast" };
        Assert.That(ib.Display(), Is.EqualTo("Chicken breast"));
    }

    [Test]
    public void Display_AlreadyCapitalized_NoChange()
    {
        var ib = new IngredientBase { Name = "Olive Oil" };
        Assert.That(ib.Display(), Is.EqualTo("Olive Oil"));
    }

    [Test]
    public void Display_SingleChar_Capitalizes()
    {
        var ib = new IngredientBase { Name = "a" };
        Assert.That(ib.Display(), Is.EqualTo("A"));
    }

    [Test]
    public void Display_EmptyName_ReturnsEmpty()
    {
        var ib = new IngredientBase { Name = string.Empty };
        Assert.That(ib.Display(), Is.EqualTo(string.Empty));
    }
}
