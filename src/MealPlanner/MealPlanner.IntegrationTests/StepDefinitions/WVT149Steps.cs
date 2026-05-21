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
            try { _wait.Until(d => !d.Url.Contains("/Login", StringComparison.OrdinalIgnoreCase)); }
            catch (WebDriverTimeoutException) { }
            _driver.Navigate().GoToUrl($"{_baseUrl}/UserSettings/Index");
            _wait.Until(driver =>
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    ?.ToString() == "complete");
        }

        [Then("a theme toggle is shown on the settings page")]
        public void ThenAThemeToggleIsShownOnTheSettingsPage()
        {
            // Navigate to the Appearance panel where the toggle lives
            var appearanceBtn = _wait.Until(driver =>
            {
                try { return driver.FindElement(By.CssSelector(".settings-nav-item[data-section='appearance']")); }
                catch (NoSuchElementException) { return null; }
            })!;
            appearanceBtn.Click();

            var toggle = _wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#themeToggle-panel"));
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException) { return null; }
            })!;

            Assert.That(toggle, Is.Not.Null);
        }

        [When("'Jack' clicks the theme toggle")]
        public void WhenJackClicksTheThemeToggle()
        {
            // Navigate to the Appearance panel where the toggle lives
            var appearanceBtn = _wait.Until(driver =>
            {
                try { return driver.FindElement(By.CssSelector(".settings-nav-item[data-section='appearance']")); }
                catch (NoSuchElementException) { return null; }
            })!;
            appearanceBtn.Click();

            var toggle = _wait.Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(By.CssSelector("#themeToggle-panel"));
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
            var isOn = js.ExecuteScript("return document.getElementById('themeToggle-panel').classList.contains('on');") as bool? == true;
            var theme = js.ExecuteScript("return document.documentElement.getAttribute('data-theme');")?.ToString();

            if (isOn)
                Assert.That(theme, Is.Null.Or.Empty);
            else
                Assert.That(theme, Is.EqualTo("light"));
        }
    }
}