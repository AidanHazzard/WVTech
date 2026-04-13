using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT149Steps
    {
        private IWebDriver _driver = null!;
        private string _baseUrl = null!;
        private WebDriverWait _wait = null!;

        // Runs before each scenerio
        [BeforeScenario]
        public void SetUp()
        {
            _driver = BDDSetup.Driver;
            _baseUrl = AUTHost.BaseUrl;
            _wait = BDDSetup.Wait;
        }

        [Given("'Jack' is on the user settings page")]
        public void GivenJackIsOnTheUserSettingsPage()
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/UserSettings/Index");
            _wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    .ToString() == "complete");
        }

        [Then("a theme toggle is shown on the settings page")]
        public void ThenAThemeToggleIsShownOnTheSettingsPage()
        {
            var toggle = _wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#themeToggle"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            Assert.That(toggle, Is.Not.Null);
        }

        [When("'Jack' clicks the theme toggle")]
        public void WhenJackClicksTheThemeToggle()
        {
            var toggle = _wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#themeToggle"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            toggle.Click();
        }

       [Then("the theme changes")]
public void ThenTheThemeChanges()
{
    var js = (IJavaScriptExecutor)_driver;
    var toggle = _driver.FindElement(By.CssSelector("#themeToggle"));
    var isChecked = js.ExecuteScript("return document.getElementById('themeToggle').checked;") as bool? == true;
    var theme = js.ExecuteScript("return document.documentElement.getAttribute('data-theme');")?.ToString();

    if (isChecked)
        Assert.That(theme, Is.EqualTo("light"));
    else
        Assert.That(theme, Is.Null.Or.Empty);
}
    }
}