using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT178Steps
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;
    private string _baseUrl = null!;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Given("{string} has upvoted the recipe {string}")]
    public void GivenUserHasUpvotedTheRecipe(string username, string recipeName)
    {
        var ctx = BDDSetup.Context;
        var userId = SharedSteps.Users[username].Id;
        var recipe = ctx.Set<Recipe>().FirstOrDefault(r => r.Name == recipeName);
        Assert.That(recipe, Is.Not.Null, $"Recipe '{recipeName}' not found in database.");

        var userRecipe = ctx.Set<UserRecipe>()
            .FirstOrDefault(ur => ur.UserId == userId && ur.RecipeId == recipe!.Id);
        if (userRecipe == null)
        {
            ctx.Add(new UserRecipe { UserId = userId, RecipeId = recipe!.Id, UserVote = UserVoteType.UpVote });
        }
        else
        {
            userRecipe.UserVote = UserVoteType.UpVote;
            ctx.Update(userRecipe);
        }
        ctx.SaveChanges();
    }

    [Given("{string} is on the recipe search page")]
    public void GivenUserIsOnRecipeSearchPage(string username)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/Recipes");
        _wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
        if (!_driver.Url.Contains("/FoodEntries/Recipes", StringComparison.OrdinalIgnoreCase))
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/FoodEntries/Recipes");
            _wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
        }
    }

    [When("{string} searches for {string}")]
    public void WhenUserSearchesFor(string username, string searchTerm)
    {
        var el = _wait.Until(d =>
        {
            try { var e = d.FindElement(By.Id("searchText")); return e.Displayed ? e : null; }
            catch (NoSuchElementException) { return null; }
        })!;
        el.Clear();
        el.SendKeys(searchTerm);
        Thread.Sleep(1100);
        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "arguments[0].dispatchEvent(new Event('input', { bubbles: true }));", el);
        Thread.Sleep(1100);
    }

    [Then("the recipe {string} appears in the search results with a rating above 0%")]
    public void ThenRecipeAppearsWithRatingAbove0(string recipeName)
    {
        var ratingEl = _wait.Until(driver =>
        {
            try
            {
                var rows = driver.FindElements(By.CssSelector(".recipeSearchRow"));
                var matchingRow = rows.FirstOrDefault(r =>
                    r.FindElements(By.CssSelector(".recipeName"))
                     .Any(el => el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)));
                if (matchingRow == null) return null;
                return matchingRow.FindElements(By.CssSelector(".recipeRating")).FirstOrDefault();
            }
            catch { return null; }
        });

        Assert.That(ratingEl, Is.Not.Null, $"Recipe '{recipeName}' not found in search results.");
        var ratingText = ratingEl!.Text.Replace("%", "").Trim();
        Assert.That(int.TryParse(ratingText, out int ratingValue), Is.True,
            $"Could not parse rating value from '{ratingEl.Text}'.");
        Assert.That(ratingValue, Is.GreaterThan(0),
            $"Expected rating above 0% for '{recipeName}' but got '{ratingEl.Text}'.");
    }
}
