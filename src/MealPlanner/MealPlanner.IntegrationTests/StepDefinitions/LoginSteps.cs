using System;
using Mealplanner.IntegrationTests;
using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

[Binding]
public class LoginSteps
{
    private readonly SharedDriver _shared;

    public const string TestUserEmail = "testuser@test.com";
    public const string TestUserPassword = "Test1234!";
    public const string TestUserName = "testuser";

    public LoginSteps(SharedDriver shared)
    {
        _shared = shared;
    }

    //error here 
    [Given("a user is logged in")]
    public void GivenAUserIsLoggedIn()
    {
        CreateTestUser();
        GivenAUserIsLoggedInAs(TestUserEmail, TestUserPassword);
    }

    [Given("a user is logged in as {string}")]
    public void GivenAUserIsLoggedInAs(string userKey)
    {
        GivenAUserIsLoggedInAs(TestUserEmail, TestUserPassword);
    }

    public static void CreateTestUser()
    {
    using var context = BDDSetup.CreateContext();
    if (context.Users.Any(u => u.Email == TestUserEmail)) return;

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

    context.Users.Add(user);
    context.SaveChanges();

    // Verify the hash works immediately after creation
    var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, TestUserPassword);
    Console.WriteLine($"Password hash verification result: {verify}");
    }

    private void GivenAUserIsLoggedInAs(string email, string password)
    {
        _shared.Driver.Navigate().GoToUrl($"{_shared.BaseUrl}/Login");
        _shared.Driver.FindElement(By.Id("Email")).SendKeys(email);
        _shared.Driver.FindElement(By.Id("Password")).SendKeys(password);
        _shared.Driver.FindElement(By.ClassName("btn")).Click();
    }
}