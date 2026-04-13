using System;
using System.Linq;
using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using MealPlanner.IntegrationTests;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT128Steps
    {
        private readonly SharedDriver _shared;
        private readonly ScenarioContext _scenarioContext;
        private int _mealId;
        private string _userId = null!;
        private string _deletedRecipeName = null!;

        public WVT128Steps(SharedDriver shared, ScenarioContext scenarioContext)
        {
            _shared = shared;
            _scenarioContext = scenarioContext;
        }

        [Given("'Jack' has a meal with recipes created")]
        public void GivenJackHasAMealWithRecipesCreated()
        {
            using var ctx = BDDSetup.CreateContext();
            _userId = ctx.Users.First(u => u.NormalizedEmail == "JACK@FAKEEMAIL.COM").Id;

            var meal = new Meal
            {
                UserId = _userId,
                Title = "Test Meal With Recipes",
                StartTime = DateTime.Now
            };

            var recipes = ctx.Set<Recipe>().Where(r => new[] { -1, -2, -3, -4 }.Contains(r.Id)).ToList();
            foreach (var recipe in recipes)
            {
                meal.Recipes.Add(recipe);
            }

            ctx.Meals.Add(meal);
            ctx.SaveChanges();
            _mealId = meal.Id;
            _scenarioContext["MealId"] = _mealId;
        }

        [Given("'Jack' is on the view meal page")]
        public void GivenJackIsOnTheViewMealPage()
        {
            if (_scenarioContext.ContainsKey("MealId"))
                _mealId = (int)_scenarioContext["MealId"];

            _shared.Driver.Navigate().GoToUrl($"{_shared.BaseUrl}/Meal/ViewMeal?id={_mealId}");
            _shared.Wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    .ToString() == "complete");
        }

        [Given("'Jack' is on the create meal page")]
        public void GivenJackIsOnTheCreateMealPage()
        {
            _shared.Driver.Navigate().GoToUrl($"{_shared.BaseUrl}/Meal/NewMeal");
            _shared.Wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    .ToString() == "complete");
        }

        [Given("'Jack' is on the edit meal page")]
        public void GivenJackIsOnTheEditMealPage()
        {
            if (_scenarioContext.ContainsKey("MealId"))
                _mealId = (int)_scenarioContext["MealId"];

            _shared.Driver.Navigate().GoToUrl($"{_shared.BaseUrl}/Meal/EditMeal?id={_mealId}");
            _shared.Wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    .ToString() == "complete");
        }

        [When("'Jack' clicks the delete button on a recipe")]
        public void WhenJackClicksTheDeleteButtonOnARecipe()
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

            var deleteBtn = firstItem.FindElement(By.CssSelector(".delete-recipe-btn"));
            deleteBtn.Click();
        }

       [When("'Jack' confirms the deletion")]
        public void WhenJackConfirmsTheDeletion()
        {
            _shared.Wait.Until(driver =>
            {
                try
                {
                    driver.SwitchTo().Alert();
                    return true;
                }
                catch (NoAlertPresentException) { return false; }
            });
            _shared.Driver.SwitchTo().Alert().Accept();
        }

        [When("'Jack' denies the deletion")]
        public void WhenJackDeniesTheDeletion()
        {
            _shared.Wait.Until(driver =>
            {
                try
                {
                    driver.SwitchTo().Alert();
                    return true;
                }
                catch (NoAlertPresentException) { return false; }
            });
            _shared.Driver.SwitchTo().Alert().Dismiss();
        }

        [Then("the recipe is removed from the meal immediately")]
        public void ThenTheRecipeIsRemovedFromTheMealImmediately()
        {
            _shared.Wait.Until(driver =>
            {
                var items = driver.FindElements(By.CssSelector(".mealRecipeItem h4"));
                return items.All(item => !item.Text.Contains(_deletedRecipeName));
            });
        }

        [Then("the recipe is still shown in the meal recipe list")]
        public void ThenTheRecipeIsStillShownInTheMealRecipeList()
        {
            _shared.Wait.Until(driver =>
            {
                var items = driver.FindElements(By.CssSelector(".mealRecipeItem h4"));
                return items.Any(item => item.Text.Contains(_deletedRecipeName));
            });
        }

        [Given("'Jack' searches for a recipe {string}")]
        [When("'Jack' searches for a recipe {string}")]
        public void GivenJackSearchesForARecipe(string searchTerm)
        {
            var searchInput = _shared.Wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#searchText"));
                    return (el.Displayed && el.Enabled) ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            ((IJavaScriptExecutor)_shared.Driver)
                .ExecuteScript("arguments[0].scrollIntoView(true);", searchInput);
            searchInput.Click();
            searchInput.Clear();
            searchInput.SendKeys(searchTerm);
            Thread.Sleep(1100);
        }

        [Given("'Jack' clicks the first search result")]
        [When("'Jack' clicks the first search result")]
        public void GivenJackClicksTheFirstSearchResult()
        {
            var firstResult = _shared.Wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector(".recipeSearchRow"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            ((IJavaScriptExecutor)_shared.Driver)
                .ExecuteScript("arguments[0].scrollIntoView(true);", firstResult);
            firstResult.Click();
        }
    }
}