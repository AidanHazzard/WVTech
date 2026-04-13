using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests
{
    [Binding]
    public class WVT140Steps
    {
        private readonly SharedDriver _shared;

        public WVT140Steps(SharedDriver shared)
        {
            _shared = shared;
        }

        [Then("an error is shown saying the recipe is already in the meal")]
        public void ThenAnErrorIsShownSayingTheRecipeIsAlreadyInTheMeal()
        {
            var alert = _shared.Wait.Until(driver =>
            {
                try
                {
                    return driver.SwitchTo().Alert();
                }
                catch (NoAlertPresentException) { return null; }
            });

            Assert.That(alert!.Text, Does.Contain("already in the meal"));
            alert.Accept();
        }
    }
}