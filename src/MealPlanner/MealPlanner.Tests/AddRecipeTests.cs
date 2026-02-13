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

        [Test]
        public void FlattenListToString()
        {
            var vm1 = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins"
            };

            string flattened = vm1.FlattenList();
            Assert.That(flattened, Is.EqualTo("1 cup sugar\n2 cups flour"));
        }

        [Test]
        public void AddsRecipeToRepository()
        {
            var vm = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins"
            };

            _controller.RecipeAdded(vm);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(1));

            var recipe = recipes.First();
            Assert.That(recipe.Name, Is.EqualTo("Test Recipe"));
            Assert.That(recipe.Ingredients, Is.EqualTo("1 cup sugar\n2 cups flour"));
            Assert.That(recipe.Directions, Is.EqualTo("Mix ingredients and bake 20 mins"));
        }

        [Test]
        public void IncorrectName()
        {
            var vm1 = new AddRecipeViewModel
            {
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
                Directions = "Mix ingredients and bake 20 mins"
            };

            var vm2 = new AddRecipeViewModel
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
            var vm1 = new AddRecipeViewModel
            {
                Name = "Test Recipe",

                Directions = "Mix ingredients and bake 20 mins"
            };

            var vm2 = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "            "
                },
                Directions = "Mix ingredients and bake 20 mins"
            };

            var vm3 = new AddRecipeViewModel
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
            var vm1 = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                Ingredients = new List<string>
                {
                    "1 cup sugar",
                    "2 cups flour"
                },
            };

            var vm2 = new AddRecipeViewModel
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
            var vm1 = new AddRecipeViewModel
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
            var vm1 = new AddRecipeViewModel
            {
                Name = "1Name",
                Ingredients = new List<string>
                {
                    "1Entry1",
                    "1Entry2"
                },
                Directions = "1Directions"
            };

            var vm2 = new AddRecipeViewModel
            {
                Name = "2Name",
                Ingredients = new List<string>
                {
                    "2Entry1",
                    "2Entry2"
                },
                Directions = "2Directions"
            };

            _controller.RecipeAdded(vm1);
            _controller.RecipeAdded(vm2);

            var recipes = _recipeRepository.ReadAll().ToList();
            Assert.That(recipes.Count, Is.EqualTo(2));

            var recipe = recipes.First();
            Assert.That(recipe.Name, Is.EqualTo("1Name"));
            Assert.That(recipe.Ingredients, Is.EqualTo("1Entry1\n1Entry2"));
            Assert.That(recipe.Directions, Is.EqualTo("1Directions"));

            var recipe2 = recipes.Last();
            Assert.That(recipe2.Name, Is.EqualTo("2Name"));
            Assert.That(recipe2.Ingredients, Is.EqualTo("2Entry1\n2Entry2"));
            Assert.That(recipe2.Directions, Is.EqualTo("2Directions"));
        }

        //handels the cleaning up after test
        [TearDown]
        public void Cleanup()
        {
            _controller?.Dispose();
            _context?.Dispose();
        }
    }
}
