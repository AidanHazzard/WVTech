using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT72Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public WVT72Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
    }

    [When("'Jack' selects meal day {string}")]
    public void WhenJackSelectsMealDate(string day)
    {
        var dayElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("SelectedDay")));
        var daySelect = new SelectElement(dayElement);
        daySelect.SelectByText(day);
    }

    [When("'Jack' enables weekly repeat")]
    public void WhenJackEnablesWeeklyRepeat()
    {
        var repeatCheckbox = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("RepeatWeekly")));

        if (!repeatCheckbox.Selected)
            repeatCheckbox.Click();
    }

    [Then("the meal repeat rule is saved as weekly")]
    public void ThenTheMealRepeatRuleIsSavedAsWeekly()
    {
        var repeatCheckbox = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("RepeatWeekly")));

        Assert.That(repeatCheckbox.Selected, Is.True);
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [When("'Jack' selects the meal day {string}")]
    public void WhenSelectsTheMealDay(string month)
    {
        var monthElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("SelectedMonth")));
        var monthSelect = new SelectElement(monthElement);
        monthSelect.SelectByText(month);
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("the meal month field is saved as {string}")]
    public void ThenTheMealMonthFieldIsSavedAs(string expectedMonth)
    {
        var monthElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("SelectedMonth")));
        var monthSelect = new SelectElement(monthElement);
        var resultMonth = monthSelect.SelectedOption;
        Assert.That(resultMonth.Text, Does.Contain(expectedMonth));
    }

    // This step definition uses Cucumber Expressions. See https://github.com/gasparnagy/CucumberExpressions.SpecFlow
    [Then("the meal day field is saved as {string}")]
    public void ThenTheMealDayFieldIsSavedAs(string expectedDay)
    {
        var dayElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElement(By.Name("SelectedDay")));
        var daySelect = new SelectElement(dayElement);
        var resultDay = daySelect.SelectedOption;
        Assert.That(resultDay.Text, Does.Contain(expectedDay));
    }
}