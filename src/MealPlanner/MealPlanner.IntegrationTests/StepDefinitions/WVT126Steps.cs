using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT126Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public WVT126Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [When("'Jack' navigates to the new meal page")]
    [Given("'Jack' is on the new meal page")]
    public void GivenJackIsOnTheNewMealPage()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/NewMeal");
    }

    [Then("the month dropdown is shown")]
    public void ThenTheMonthDropdownIsShown()
    {
        var monthDropdown = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("SelectedMonth")));

        Assert.That(monthDropdown.Displayed, Is.True);
    }

    [Then("the day dropdown is shown")]
    public void ThenTheDayDropdownIsShown()
    {
        var dayDropdown = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("SelectedDay")));

        Assert.That(dayDropdown.Displayed, Is.True);
    }

    [When("'Jack' enters a meal title {string}")]
    public void WhenJackEntersAMealTitle(string title)
    {
        var titleInput = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.CssSelector("#Title.mealInput")));

        titleInput.Clear();
        titleInput.SendKeys(title);
    }

    [When("'Jack' selects month {string}")]
    public void WhenJackSelectsMonth(string month)
    {
        var monthElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("SelectedMonth")));
        new SelectElement(monthElement).SelectByText(month);
    }

    [When("'Jack' selects day {string}")]
    public void WhenJackSelectsDay(string day)
    {
        var dayElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Id("SelectedDay")));
        new SelectElement(dayElement).SelectByText(day);
    }

    [When("'Jack' saves the meal")]
    public void WhenJackSavesTheMeal()
    {
        var saveButton = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.CssSelector("#createMeal")));
        
        new Actions(_driver).ScrollToElement(saveButton).Perform();
        saveButton.Click();
    }

    [Then("the meal creation form submits successfully")]
    public void ThenTheMealFormSubmitsSuccessfully()
    {
        new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => !d.Url.Contains("NewMeal"));

        Assert.That(_driver.Url.Contains("NewMeal"), Is.False);
    }

    [Then("the edit meal form submits successfully")]
    public void ThenTheEditMealFormSubmitsSuccessfully()
    {
        new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.PageSource.Contains("Meal updated successfully"));
        Assert.That(_driver.PageSource, Does.Contain("Meal updated successfully"));
    }
}