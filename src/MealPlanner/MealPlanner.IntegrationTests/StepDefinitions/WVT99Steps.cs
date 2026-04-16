using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using NUnit.Framework;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT99Steps
{
    IWebDriver _driver;
    string _baseUrl;
    int _recipeId;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Given("{string} selects the predefined tag {string}")]
    [When("{string} selects the predefined tag {string}")]
    public void GivenUserSelectsPredefinedTag(string username, string tag)
    {
        var select = new SelectElement(_driver.FindElement(By.Id("tag-select")));
        select.SelectByValue(tag);
    }

    [Given("{string} adds the custom tag {string}")]
    public void GivenUserAddsCustomTag(string username, string tag)
    {
        var input = _driver.FindElement(By.Id("custom-tag-input"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", input);
        input.Clear();
        input.SendKeys(tag);
        var btn = _driver.FindElement(By.Id("add-custom-tag-btn"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
    }

    [Given("{string} has a recipe named {string} with the tag {string}")]
    public void GivenUserHasRecipeWithTag(string username, string recipeName, string tagName)
    {
        var ctx = BDDSetup.Context;
        var userId = SharedSteps.Users[username].Id;

        // Clean up any leftover recipe from previous runs
        var existing = ctx.Set<Recipe>()
            .Include(r => r.Tags)
            .FirstOrDefault(r => r.Name == recipeName);
        if (existing != null)
        {
            ctx.Remove(existing);
            ctx.SaveChanges();
        }

        // Find or create the tag
        var tag = ctx.Set<Tag>().FirstOrDefault(t => t.Name == tagName)
                  ?? new Tag { Name = tagName };

        var recipe = new Recipe
        {
            Name = recipeName,
            Directions = "Test directions",
            Calories = 200,
            Protein = 10,
            Fat = 5,
            Carbs = 30,
            Tags = [tag]
        };

        ctx.Add(recipe);
        ctx.SaveChanges();
        _recipeId = recipe.Id;

        ctx.Add(new UserRecipe
        {
            UserId = userId,
            RecipeId = _recipeId,
            UserOwner = true,
            UserFavorite = false,
            UserVote = UserVoteType.NoVote
        });
        ctx.SaveChanges();
    }

    [When("{string} views the recipe detail page for {string}")]
    public void WhenUserViewsRecipeDetailPage(string username, string recipeName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/Recipes/{_recipeId}");
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("the tag {string} is visible on the page")]
    public void ThenTagIsVisibleOnPage(string tag)
    {
        Assert.That(_driver.PageSource, Does.Contain(tag));
    }

    [Then("the recipe {string} has the tag {string} in the database")]
    public void ThenRecipeHasTagInDatabase(string recipeName, string tagName)
    {
        var ctx = BDDSetup.Context;
        var recipe = ctx.Set<Recipe>()
            .Include(r => r.Tags)
            .FirstOrDefault(r => r.Name == recipeName);

        Assert.That(recipe, Is.Not.Null, $"Recipe '{recipeName}' not found in database.");
        Assert.That(recipe!.Tags.Any(t => t.Name == tagName), Is.True,
            $"Recipe '{recipeName}' does not have tag '{tagName}'.");
    }

    [Given("{string} is on the edit recipe page for {string}")]
    public void GivenUserIsOnEditRecipePageFor(string username, string recipeName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/EditRecipe/{_recipeId}");
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [When("{string} submits the edit recipe form")]
    public void WhenUserSubmitsEditRecipeForm(string username)
    {
        var btn = _driver.FindElement(By.CssSelector("button.buttonBlue[type='submit']"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", btn);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }
}
