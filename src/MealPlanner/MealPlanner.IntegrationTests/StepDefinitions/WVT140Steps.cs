using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT140Steps
    {
        private WebDriverWait _wait = null!;

        // Runs before each scenerio
        [BeforeScenario]
        public void SetUp()
        {
            _wait = BDDSetup.Wait;
        }

        [Then("an error is shown saying the recipe is already in the meal")]
        public void ThenAnErrorIsShownSayingTheRecipeIsAlreadyInTheMeal()
        {
            var message = _wait.Until(driver =>
            {
                var msg = ((IJavaScriptExecutor)driver).ExecuteScript("return window._alertMessage;");
                return msg?.ToString();
            });

            Assert.That(message, Does.Contain("already in the meal"));
        }
    }
}