using MealPlanner.Controllers;
using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class EditRecipeTests
    {
        private MealPlannerDBContext _context;
        private RecipeRepository _recipeRepository;
        private FoodEntriesController _controller;

        //sets up an in memory database to play with and sets the variables above to a new instance
        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<MealPlannerDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            _context = new MealPlannerDBContext(options);
            _recipeRepository = new RecipeRepository(_context);
            var userRecipeRepo = new Mock<IUserRecipeRepository>();
            var registrationService = new Mock<IRegistrationService>();
            _controller = new FoodEntriesController(_recipeRepository, userRecipeRepo.Object, _context, registrationService.Object);

            var vm = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = ["sugar", "flour"],
                IngredientAmounts = [1, 2],
                IngredientMeasurements = ["cup", "cups"],
                Directions = "Mix ingredients and bake 20 mins",
                Calories = 0,
                Protein = 1,
                Carbs = 2,
                Fat = 3,
            };

            await _controller.RecipeAdded(vm);
        }

        //handels the cleaning up after every test
        [TearDown]
        public void Cleanup()
        {
            _controller?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public async Task EditTest()
        {
            var vm = new RecipeViewModel
            {
                Name = "Edited Test Recipe",
                Ingredients = ["sugar", "candy"],
                IngredientAmounts = [2, 2],
                IngredientMeasurements = ["ounces", "cups"],
                Directions = "Edited Mix ingredients and bake 20 mins",
                Calories = 1,
                Protein = 2,
                Carbs = 3,
                Fat = 4,
            };

            await _controller.RecipeEditFinished(vm, 1);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(1));

            var recipe = recipes.First();
            Assert.That(recipe.Name, Is.EqualTo("Edited Test Recipe"));
            Assert.That(recipe.Directions, Is.EqualTo("Edited Mix ingredients and bake 20 mins"));
            Assert.That(recipe.Ingredients.Count, Is.EqualTo(2));
            Assert.That(recipe.Ingredients[0].IngredientBase.Name, Is.EqualTo("sugar"));
            Assert.That(recipe.Ingredients[1].IngredientBase.Name, Is.EqualTo("candy"));
            Assert.That(recipe.Ingredients[0].ToString(), Is.EqualTo("2 ounces of sugar"));
            Assert.That(recipe.Ingredients[1].ToString(), Is.EqualTo("2 cups of candy"));
            Assert.That(recipe.Calories, Is.EqualTo(1));
            Assert.That(recipe.Protein, Is.EqualTo(2));
            Assert.That(recipe.Carbs, Is.EqualTo(3));
            Assert.That(recipe.Fat, Is.EqualTo(4));
        }

        public async Task InvalidModelState()
        {
            var vm = new RecipeViewModel
            {
                Name = "Edited Test Recipe",
                Ingredients = ["sugar", "flour"],
                IngredientAmounts = [1, 2],
                IngredientMeasurements = ["cup", "cups"],
                Directions = "Mix ingredients and bake 20 mins",
                Calories = 1,
                Protein = 2,
                Carbs = 3,
                Fat = 4,
            };

            _controller.ModelState.AddModelError("Name", "Required");
            await _controller.RecipeEditFinished(vm, 1);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(1));

            var recipe = recipes.First();
            Assert.That(recipe.Name, Is.EqualTo("Test Recipe"));
        }
    }
}