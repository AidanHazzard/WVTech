using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using NUnit.Framework;

[Binding]
public class WVT127Steps
{
     IWebDriver _driver;
     string _updatedTitle = "Updated Meal Title";

    // Runs before each scenario
    [BeforeScenario]
    public void SetUp()
    {
        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");

        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
    }

    // Runs after each scenario
    [AfterScenario]
    public void TearDown()
    {
        _driver.Quit();
    }

    [Given("a user is on the edit meal page")]
    public void GivenAUserIsOnTheEditMealPage()
    {
        // Adjust ID or route as needed
        _driver.Navigate().GoToUrl("http://localhost:5124/Meals/EditMeal/{mealId}");
    }

    [Then("all associated recipes are displayed")]
    public void ThenAllAssociatedRecipesAreDisplayed()
    {
        ReadOnlyCollection<IWebElement> recipes =
            _driver.FindElements(By.ClassName("meal-recipe-row"));

        Assert.That(recipes, Is.Not.Empty);

        foreach (var recipe in recipes)
        {
            Assert.That(recipe.Displayed, Is.True);
        }
    }

    [When("User updates the meal title")]
    public void WhenUserUpdatesTheMealTitle()
    {
        IWebElement titleInput = _driver.FindElement(By.Id("mealTitle"));
        titleInput.Clear();
        titleInput.SendKeys(_updatedTitle);
    }

    [Then("the updated meal title is shown immediately")]
    public void ThenTheUpdatedMealTitleIsShownImmediately()
    {
        IWebElement titleInput = _driver.FindElement(By.Id("mealTitle"));
        string currentValue = titleInput.GetAttribute("value");

        Assert.That(currentValue, Is.EqualTo(_updatedTitle));
    }

    [When("User saves the meal")]
    public void WhenUserSavesTheMeal()
    {
        IWebElement saveButton = _driver.FindElement(By.Id("saveMealButton"));
        saveButton.Click();
    }

    [Then("the meal is saved with the updated title")]
    public void ThenTheMealIsSavedWithTheUpdatedTitle()
    {
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElement(By.Id("mealTitle")).Displayed);

        IWebElement titleInput = _driver.FindElement(By.Id("mealTitle"));
        string savedTitle = titleInput.GetAttribute("value");

        Assert.That(savedTitle, Is.EqualTo(_updatedTitle));
    }
}