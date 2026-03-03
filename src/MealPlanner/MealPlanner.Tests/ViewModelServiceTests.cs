using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Moq;

namespace MealPlanner.Tests;

[TestFixture]
public class ViewModelServiceTests
{
    [Test]
    public void RecipeFromRecipeVM_ReturnsRecipe()
    {
        // Arrange
        RecipeViewModel vm = new RecipeViewModel
        {
            Name = "name",
            Directions = "dir",
            Ingredients = ["i1_Base", "i2_Base"],
            IngredientAmounts = [1,1],
            IngredientMeasurements = ["i1_Measure", "i2_Measure"],
            Calories = 1,
            Protein = 2,
            Carbs = 3,
            Fat = 4
        };

        // Act
        Recipe r = ViewModelService.RecipeFromRecipeVM(vm);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(r.Name, Is.EqualTo("name"));
            Assert.That(r.Directions, Is.EqualTo("dir"));
            Assert.That(r.Calories, Is.EqualTo(1));
            Assert.That(r.Protein, Is.EqualTo(2));
            Assert.That(r.Carbs, Is.EqualTo(3));
            Assert.That(r.Fat, Is.EqualTo(4));
            Assert.That(r.Ingredients[0].Amount, Is.EqualTo(1));
            Assert.That(r.Ingredients[0].IngredientBase.Name, Is.EqualTo("i1_Base"));
            Assert.That(r.Ingredients[0].Measurement.Name, Is.EqualTo("i1_Measure"));
            Assert.That(r.Ingredients[1].Amount, Is.EqualTo(1));
            Assert.That(r.Ingredients[1].IngredientBase.Name, Is.EqualTo("i2_Base"));
            Assert.That(r.Ingredients[1].Measurement.Name, Is.EqualTo("i2_Measure"));
        }
    }

    [Test]
    public void RecipeToRecipeVM_ReturnsViewModel()
    {
        // Arrange
        Ingredient i1 = new Ingredient
        {
            Amount = 1,
            IngredientBase = new IngredientBase { Name = "i1_base"},
            Measurement = new Measurement { Name = "i1_measure" }
        };
        Ingredient i2 = new Ingredient
        {
            Amount = 2,
            IngredientBase = new IngredientBase { Name = "i2_base"},
            Measurement = new Measurement { Name = "i2_measure" }
        };

        Recipe r = new Recipe
        {
            Name = "name",
            Directions = "dir",
            Ingredients = [i1, i2],
            Calories = 1,
            Protein = 2,
            Carbs = 3,
            Fat = 4
        };

        // Act
        RecipeViewModel vm = ViewModelService.RecipeToRecipeVM(r);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.Name, Is.EqualTo("name"));
            Assert.That(vm.Directions, Is.EqualTo("dir"));
            Assert.That(vm.Calories, Is.EqualTo(1));
            Assert.That(vm.Protein, Is.EqualTo(2));
            Assert.That(vm.Carbs, Is.EqualTo(3));
            Assert.That(vm.Fat, Is.EqualTo(4));
            Assert.That(vm.IngredientAmounts[0], Is.EqualTo(1));
            Assert.That(vm.Ingredients[0], Is.EqualTo("i1_base"));
            Assert.That(vm.IngredientMeasurements[0], Is.EqualTo("i1_measure"));
            Assert.That(vm.IngredientAmounts[1], Is.EqualTo(2));
            Assert.That(vm.Ingredients[1], Is.EqualTo("i2_base"));
            Assert.That(vm.IngredientMeasurements[1], Is.EqualTo("i2_measure"));
        }
    }
}