using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT34Steps
{
    IWebDriver _driver;
    string _baseUrl;
    
    // Runs before each scenerio
    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("he sees the recipe has a rating of {string}")]
    public void ThenHeSeesTheRecipeHasARatingOf(string rating)
    {
        IWebElement voteElement = _driver.FindElement(By.Id("votePercent"));
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
        wait.Until(_ => voteElement.Text == rating);
        Assert.That(voteElement.Text, Is.EqualTo(rating));
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("he upvotes the recipe")]    
    [When("she upvotes the recipe")]
    public void WhenTheyUpvotesTheRecipe()
    {
        _driver.FindElement(By.Id("thumbs-up")).Click();
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("{string} had upvoted the recipe with id {int}")]
    public void GivenHadUpvotedTheRecipeWithId(string userName, int recipeId)
    {
        using var ctx = BDDSetup.CreateContext();
        UserRecipe userRecipe = ctx.Find<UserRecipe>(SharedSteps.Users[userName].Id, recipeId)
            ?? new UserRecipe()
            {
                User = SharedSteps.Users[userName],
                Recipe = ctx.Find<Recipe>(recipeId)
            };
        userRecipe.UserVote = UserVoteType.UpVote;
        ctx.Update(userRecipe);
        ctx.SaveChanges();
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("he downvotes the recipe")]
    public void WhenHeDownvotesTheRecipe()
    {
        _driver.FindElement(By.Id("thumbs-down")).Click();
    }
}