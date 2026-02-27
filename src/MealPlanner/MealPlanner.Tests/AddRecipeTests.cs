using System;
using System.Linq;
using MealPlanner.Controllers;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class AddRecipeTests
    {
        private MealPlannerDBContext _context;
        private RecipeRepository _recipeRepository;
        private FoodEntriesController _controller;

        //sets up an in memory database to play with and sets the variables above to a new instance
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MealPlannerDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            _context = new MealPlannerDBContext(options);
            _recipeRepository = new RecipeRepository(_context);
            _controller = new FoodEntriesController(_recipeRepository, _context);
        }

        //handels the cleaning up after every test
        [TearDown]
        public void Cleanup()
        {
            _controller?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public void FlattenListToString()
        {
            var vm1 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins"
                //no need to set nutrition because there are auto set to 0 which is acceptable                
            };

            string flattened = vm1.FlattenList();
            Assert.That(flattened, Is.EqualTo("1 cup sugar\n2 cups flour"));
        }

        [Test]
        public void AddsRecipeToRepository()
        {
            var vm = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins",
                Calories = 0,
                Protein = 1,
                Carbs = 2,
                Fat = 3,
            };

            _controller.RecipeAdded(vm);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(1));

            var recipe = recipes.First();
            Assert.That(recipe.Name, Is.EqualTo("Test Recipe"));
            Assert.That(recipe.Directions, Is.EqualTo("Mix ingredients and bake 20 mins"));
            Assert.That(recipe.Ingredients[0].IngredientBase.Name, Is.EqualTo("1 cup sugar"));
            Assert.That(recipe.Ingredients[1].IngredientBase.Name, Is.EqualTo("2 cups flour"));
            Assert.That(recipe.Calories, Is.EqualTo(0));
            Assert.That(recipe.Protein, Is.EqualTo(1));
            Assert.That(recipe.Carbs, Is.EqualTo(2));
            Assert.That(recipe.Fat, Is.EqualTo(3));
        }

        [Test]
        public void IncorrectName()
        {
            var vm1 = new RecipeViewModel
            {
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins"
                //no need to set the cals and stuff because that is auto set to 0 which is valid
            };

            var vm2 = new RecipeViewModel
            {
                Name = "    ",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins"
            };
            _controller.RecipeAdded(vm1);
            _controller.RecipeAdded(vm2);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(0));
        }

        [Test]
        public void IncorrectIngredients()
        {
            var vm1 = new RecipeViewModel
            {
                Name = "Test Recipe",

                Directions = "Mix ingredients and bake 20 mins"
            };

            var vm2 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "            "
                },
                Directions = "Mix ingredients and bake 20 mins"
            };

            var vm3 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>(),

                Directions = "Mix ingredients and bake 20 mins"
            };
            _controller.RecipeAdded(vm1);
            _controller.RecipeAdded(vm2);
            _controller.RecipeAdded(vm3);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(0));
        }

        [Test]
        public void IncorrectDirections()
        {
            var vm1 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
            };

            var vm2 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "            "
            };
            _controller.RecipeAdded(vm1);
            _controller.RecipeAdded(vm2);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(0));
        }

        [Test]
        public void InvalidModelState()
        {
            var vm1 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins"
            };

            _controller.ModelState.AddModelError("Name", "Required");
            _controller.RecipeAdded(vm1);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddingMultipleRecipes()
        {
            var vm1 = new RecipeViewModel
            {
                Name = "1Name",
                Ingredients = new List<string>
                {
                    "1Entry1",
                    "1Entry2"
                },
                Directions = "1Directions",
                Calories = 0,
                Protein = 1,
                Carbs = 2,
                Fat = 3,
            };

            var vm2 = new RecipeViewModel
            {
                Name = "2Name",
                Ingredients = new List<string>
                {
                    "2Entry1",
                    "2Entry2"
                },
                Directions = "2Directions",
                Calories = 20,
                Protein = 21,
                Carbs = 22,
                Fat = 23,
            };

            _controller.RecipeAdded(vm1);
            _controller.RecipeAdded(vm2);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(2));

            var recipe = recipes.First();
            Assert.That(recipe.Name, Is.EqualTo("1Name"));
            Assert.That(recipe.Directions, Is.EqualTo("1Directions"));
            Assert.That(recipe.Ingredients[0].IngredientBase.Name, Is.EqualTo("1Entry1"));
            Assert.That(recipe.Ingredients[1].IngredientBase.Name, Is.EqualTo("1Entry2"));
            Assert.That(recipe.Calories, Is.EqualTo(0));
            Assert.That(recipe.Protein, Is.EqualTo(1));
            Assert.That(recipe.Carbs, Is.EqualTo(2));
            Assert.That(recipe.Fat, Is.EqualTo(3));

            var recipe2 = recipes.Last();
            Assert.That(recipe2.Name, Is.EqualTo("2Name"));
            Assert.That(recipe2.Directions, Is.EqualTo("2Directions"));
            Assert.That(recipe2.Ingredients[0].IngredientBase.Name, Is.EqualTo("2Entry1"));
            Assert.That(recipe2.Ingredients[1].IngredientBase.Name, Is.EqualTo("2Entry2"));
            Assert.That(recipe2.Calories, Is.EqualTo(20));
            Assert.That(recipe2.Protein, Is.EqualTo(21));
            Assert.That(recipe2.Carbs, Is.EqualTo(22));
            Assert.That(recipe2.Fat, Is.EqualTo(23));
        }

        [Test]
        public void NutritionSetToANegative()
        {
            var vm1 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins",
                Calories = -1,
                Protein = 0,
                Carbs = 0,
                Fat = 0
            };

            var vm2 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins",
                Calories = 0,
                Protein = -1,
                Carbs = 0,
                Fat = 0
            };

            var vm3 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins",
                Calories = 0,
                Protein = 0,
                Carbs = -1,
                Fat = 0
            };

            var vm4 = new RecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins",
                Calories = 0,
                Protein = 0,
                Carbs = 0,
                Fat = -1
            };
            _controller.RecipeAdded(vm1);
            _controller.RecipeAdded(vm2);
            _controller.RecipeAdded(vm3);
            _controller.RecipeAdded(vm4);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(0));
        }
    }
}
