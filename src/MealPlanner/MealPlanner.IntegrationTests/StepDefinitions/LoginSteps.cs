using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class LoginSteps
{
    private static IWebDriver _driver = null!;
    private static WebDriverWait _wait = null!;
    private string _baseUrl = null!;
    
    public const string TestUserEmail = "testuser@test.com";
    public const string TestUserPassword = "Test1234!";
    public const string TestUserName = "testuser";

    // Runs before each scenerio
    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    //error here 
    [Given("a user is logged in")]
    public void GivenAUserIsLoggedIn()
    {
        CreateTestUser();
        Login(TestUserEmail, TestUserPassword);
    }

    [Given("a user is logged in as {string}")]
    public void GivenAUserIsLoggedInAs(string userKey)
    {
        Login(TestUserEmail, TestUserPassword);
    }

    public static void CreateTestUser()
    {
        using var ctx = BDDSetup.CreateContext();
        if (ctx.Users.Any(u => u.Email == TestUserEmail)) return;

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = TestUserEmail,
            NormalizedUserName = TestUserEmail.ToUpper(),
            Email = TestUserEmail,
            NormalizedEmail = TestUserEmail.ToUpper(),
            EmailConfirmed = true,
            FullName = "Test User",
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        // Use PasswordHasher with the exact same user object that will be used for verification
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, TestUserPassword);

        ctx.Users.Add(user);
        ctx.SaveChanges();

        // Verify the hash works immediately after creation
        var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, TestUserPassword);
        Console.WriteLine($"Password hash verification result: {verify}");
    }

    private void Login(string email, string password)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Login");
        _wait.Until(d => d.Url == $"{_baseUrl}/Login");
        _driver.FindElement(By.Id("Email")).SendKeys(email);
        _driver.FindElement(By.Id("Password")).SendKeys(password);
        _driver.FindElement(By.ClassName("btn")).Click();
    }
}