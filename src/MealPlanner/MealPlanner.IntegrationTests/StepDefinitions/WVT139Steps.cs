using System;
using System.Linq;
using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT129Steps
    {
        private readonly SharedDriver _shared;
        private string _deletedRecipeName = null!;
        private string _deletedRecipeId = null!;
        private string _userId = null!;

        public WVT129Steps(SharedDriver shared)
        {
            _shared = shared;
        }

        [Given("'Jack' has a recipe created")]
        public void GivenJackHasARecipeCreated()
        {
            using var ctx = BDDSetup.CreateContext();
            _userId = ctx.Users.First(u => u.NormalizedEmail == "JACK@FAKEEMAIL.COM").Id;

            // Clean up any existing test recipes for this user
            var existingUserRecipes = ctx.Set<UserRecipe>()
                .Where(ur => ur.UserId == _userId && ur.UserOwner == true)
                .ToList();
            var existingRecipeIds = existingUserRecipes.Select(ur => ur.RecipeId).ToList();
            ctx.Set<UserRecipe>().RemoveRange(existingUserRecipes);
            var existingRecipes = ctx.Recipes
                .Where(r => existingRecipeIds.Contains(r.Id))
                .ToList();
            ctx.Recipes.RemoveRange(existingRecipes);
            ctx.SaveChanges();

            var recipe = new Recipe
            {
                Name = "Jack's Test Recipe",
                Directions = "Test directions"
            };

            ctx.Recipes.Add(recipe);
            ctx.SaveChanges();

            var userRecipe = new UserRecipe
            {
                UserId = _userId,
                RecipeId = recipe.Id,
                UserOwner = true,
                UserVote = UserVoteType.UpVote
            };

            ctx.Set<UserRecipe>().Add(userRecipe);
            ctx.SaveChanges();
        }

        [Given("'Jack' is on the recipe page")]
        public void GivenJackIsOnTheRecipePage()
        {
            _shared.Driver.Navigate().GoToUrl($"{_shared.BaseUrl}/FoodEntries/Recipes");
            _shared.Wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    .ToString() == "complete");
        }

        [When("'Jack' clicks the delete button on their recipe")]
        public void WhenJackClicksTheDeleteButtonOnTheirRecipe()
        {
            var firstItem = _shared.Wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector(".mealRecipeItem"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            _deletedRecipeName = firstItem.FindElement(By.CssSelector("h4")).Text;
            _deletedRecipeId = firstItem.GetAttribute("data-recipe-id");

            var deleteBtn = firstItem.FindElement(By.CssSelector(".delete-recipe-btn"));
            deleteBtn.Click();
        }

        [Then("the recipe is removed from the recipe list")]
        public void ThenTheRecipeIsRemovedFromTheRecipeList()
        {
            var js = (IJavaScriptExecutor)_shared.Driver;
            var wait = new WebDriverWait(_shared.Driver, TimeSpan.FromSeconds(15));

            // Wait for fetch to complete
            wait.Until(driver => js.ExecuteScript("return window._deleteStatus != null;") as bool? == true);

            _shared.Driver.Navigate().GoToUrl($"{_shared.BaseUrl}/FoodEntries/Recipes");
            wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    .ToString() == "complete");

            var items = _shared.Driver.FindElements(By.CssSelector(".mealRecipeItem"));
            Assert.That(items.All(item => item.GetAttribute("data-recipe-id") != _deletedRecipeId), Is.True);
        }

        [Then("the recipe is still shown in the recipe list")]
        public void ThenTheRecipeIsStillShownInTheRecipeList()
        {
            _shared.Wait.Until(driver =>
            {
                var items = driver.FindElements(By.CssSelector(".mealRecipeItem h4"));
                return items.Any(item => item.Text.Contains(_deletedRecipeName));
            });
        }
    }
}