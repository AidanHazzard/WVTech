using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using NUnit.Framework;
using Mealplanner.IntegrationTests;
using MealPlanner.IntegrationTests;

namespace Mealplanner.IntegrationTests;

[Binding]
public class AddRecipeSteps
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

    [Then("{string} remains on the create recipe page")]
    public void ThenUserRemainsOnCreateRecipePage(string username)
    {
        Assert.That(_driver.Url, Does.Contain("/FoodEntries/AddNewRecipe"));
    }

    [Then("{string} is redirected away from the create recipe page")]
    public void ThenUserIsRedirectedAwayFromCreateRecipePage(string username)
    {
        Assert.That(_driver.Url, Does.Not.Contain("/FoodEntries/AddNewRecipe"));
    }

    [Given("{string} adds a blank ingredient")]
    public void GivenUserAddsABlankIngredient(string username)
    {
        _driver.FindElement(By.Id("buttonAppend")).Click();
    }
}