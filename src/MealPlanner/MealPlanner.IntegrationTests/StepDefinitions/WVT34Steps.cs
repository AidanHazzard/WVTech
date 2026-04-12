using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT34Steps
{
    IWebDriver _driver;
    string _baseUrl;
    readonly string _userPassword = "1234!Abcd";
    readonly string _emailBase = "@fakeemail.com";
    Dictionary<string, User> users = [];
    
    // Runs before each scenerio
    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [AfterScenario]
    public void TearDown()
    {
        try
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/UserSettings");
            _driver.FindElement(By.TagName("form")).Click();
        }
        catch (Exception)
        {
            // Ignore exceptions during teardown to avoid masking test results
        }
      
    }
       


    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("there is a user named {string}")]
    public void GivenThereIsAUserNamed(string userName)
    {
        using var context = BDDSetup.CreateContext();
        string email = $"{userName}{_emailBase}";
        var existing = context.Set<User>().FirstOrDefault(u => u.NormalizedEmail == email.ToUpper());
        if (existing != null)
        {
            users[userName] = existing;
            return;
        }

        User newUser = new User()
        {
            FullName = userName,
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        newUser.PasswordHash = new PasswordHasher<User>().HashPassword(newUser, _userPassword);
        users.Add(userName, newUser);
        context.Add(newUser);
        context.SaveChanges();
    }
 
    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("{string} is logged into Onebite")]
    public void GivenIsLoggedIntoOnebite(string userName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Login");
        _driver.FindElement(By.Id("Email")).SendKeys($"{userName}{_emailBase}");
        _driver.FindElement(By.Id("Password")).SendKeys(_userPassword);
        _driver.FindElement(By.ClassName("btn")).Click();
        
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("he is on the {string} page")]  
    [Given("she is on the {string} page")]  
    [When("navigates to the {string} page")]
    public void GivenTheyAreOnThePage(string pagePath)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/{pagePath}");
        new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(d => d.Url.Contains(pagePath));
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
        using var context = BDDSetup.CreateContext();
        UserRecipe userRecipe = context.Find<UserRecipe>(users[userName].Id, recipeId)
            ?? new UserRecipe()
            {
                User = users[userName],
                Recipe = context.Find<Recipe>(recipeId)
            };
        userRecipe.UserVote = UserVoteType.UpVote;
        context.Update(userRecipe);
        context.SaveChanges();
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("he downvotes the recipe")]
    public void WhenHeDownvotesTheRecipe()
    {
        _driver.FindElement(By.Id("thumbs-down")).Click();
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("{string} had downvoted the recipe with id {int}")]
    public void GivenHadDownvotedTheRecipeWithId(string userName, int recipeId)
    {
        using var context = BDDSetup.CreateContext();
        UserRecipe userRecipe = context.Find<UserRecipe>(users[userName].Id, recipeId)
            ?? new UserRecipe()
            {
                User = users[userName],
                Recipe = context.Find<Recipe>(recipeId)
            };
        userRecipe.UserVote = UserVoteType.DownVote;
        context.Update(userRecipe);
        context.SaveChanges();
    }
}