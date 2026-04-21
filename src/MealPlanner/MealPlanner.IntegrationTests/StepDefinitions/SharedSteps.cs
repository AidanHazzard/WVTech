using OpenQA.Selenium;
using Reqnroll;
using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using OpenQA.Selenium.Support.UI;

namespace Mealplanner.IntegrationTests;

[Binding]
public class SharedSteps
{
    IWebDriver _driver;
    string _baseUrl;
    MealPlannerDBContext _context;
    readonly string _userPassword = "1234!Abcd";
    readonly string _emailBase = "@fakeemail.com";
    public static Dictionary<string, User> Users = [];
    
    // Runs before each scenerio
    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _context = BDDSetup.Context;
    }

    [AfterScenario]
    public void LogOut()
    {
        try
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/UserSettings");
            _driver.FindElement(By.TagName("form")).Click();
        }
        catch
        {}
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("there is a user named {string}")]
    public void GivenThereIsAUserNamed(string userName)
    {
        string email = $"{userName}{_emailBase}";
        if (_context.Set<User>().Where(u => u.NormalizedEmail == email.ToUpper()).Any()) return;

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
        Users.Add(userName, newUser);
        _context.Add(newUser);
        _context.SaveChanges();
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
    [When("he navigates to the {string} page")]
    public void GivenTheyAreOnThePage(string pagePath)
    {
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        _driver.Navigate().GoToUrl($"{_baseUrl}/{pagePath}");
        wait.Until(d => d.Url == $"{_baseUrl}/{pagePath}");
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Given("{string} had downvoted the recipe with id {int}")]
    public void GivenHadDownvotedTheRecipeWithId(string userName, int recipeId)
    {
        UserRecipe? userRecipe = _context.Find<UserRecipe>(Users[userName].Id, recipeId);
        if (userRecipe == null)
        {
            userRecipe = new UserRecipe()
            {
                UserId = Users[userName].Id,
                RecipeId = recipeId,
                UserVote = UserVoteType.DownVote
            };
            
            _context.Add(userRecipe);
            _context.SaveChanges();
        }
        else if (userRecipe.UserVote != UserVoteType.DownVote)
        {
            userRecipe.UserVote = UserVoteType.DownVote;
            _context.Update(userRecipe);
            _context.SaveChanges();
        }
    }
}