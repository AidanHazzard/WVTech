using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using NUnit.Framework;

namespace Mealplanner.IntegrationTests;

[Binding]
public class ThemeChangeSteps
{
    IWebDriver _driver;
    string _baseUrl;
    readonly string _emailBase = "@fakeemail.com";

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [When("{string} clicks the change theme button")]
    public void WhenUserClicksChangeThemeButton(string username)
    {
        var link = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.LinkText("Change Theme"));
                return el.Displayed ? el : null;
            }
            catch (NoSuchElementException) { return null; }
        })!;

        link.Click();

        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver =>
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("{string} has dark theme enabled in the database")]
    public void ThenUserHasDarkThemeEnabled(string username)
    {
        using var ctx = BDDSetup.CreateContext();
        var userId = ctx.Users
            .First(u => u.NormalizedEmail == $"{username}{_emailBase}".ToUpper()).Id;
        var profile = ctx.Set<UserProfile>().FirstOrDefault(p => p.UserId == userId);
        Assert.That(profile, Is.Not.Null, "UserProfile row does not exist for this user");
        Assert.That(profile!.IsDarkTheme, Is.True);
    }


    [Then("{string} has light theme enabled in the database")]
    public void ThenUserHasLightThemeEnabled(string username)
    {
        using var ctx = BDDSetup.CreateContext();
        var userId = ctx.Users
            .First(u => u.NormalizedEmail == $"{username}{_emailBase}".ToUpper()).Id;
        var profile = ctx.Set<UserProfile>().FirstOrDefault(p => p.UserId == userId);
        Assert.That(profile, Is.Not.Null, "UserProfile row does not exist for this user");
        Assert.That(profile!.IsDarkTheme, Is.False);
    }

    [Given("{string} has a user profile")]
    public void GivenUserHasAUserProfile(string username)
    {
        using var ctx = BDDSetup.CreateContext();
        var userId = ctx.Users
            .First(u => u.NormalizedEmail == $"{username}{_emailBase}".ToUpper()).Id;

        var existing = ctx.Set<UserProfile>().FirstOrDefault(p => p.UserId == userId);
        if (existing == null)
        {
            ctx.Add(new UserProfile
            {
                UserId = userId,
                IsDarkTheme = false
            });
            ctx.SaveChanges();
        }
    }
}