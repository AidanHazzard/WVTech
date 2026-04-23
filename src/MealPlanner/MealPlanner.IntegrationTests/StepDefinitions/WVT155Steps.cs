using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT155Steps
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private string _clickedRecipeId = string.Empty;

    public WVT155Steps()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
    }

    [When("'Jack' clicks on a recipe name in the meal")]
    public void WhenJackClicksOnARecipeNameInTheMeal()
    {
        var firstItem = _wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector(".mealRecipeItem"));
                return el.Displayed ? el : null;
            }
            catch (NoSuchElementException) { return null; }
        })!;

        // ViewMeal uses data-recipe-id; EditMeal uses data-id
        _clickedRecipeId = firstItem.GetAttribute("data-recipe-id")
                           ?? firstItem.GetAttribute("data-id")
                           ?? string.Empty;

        firstItem.FindElement(By.CssSelector("h4")).Click();
    }

    [Then("'Jack' is navigated to that recipe's detail page")]
    public void ThenJackIsNavigatedToThatRecipesDetailPage()
    {
        _wait.Until(driver => driver.Url.Contains($"/FoodEntries/Recipes/{_clickedRecipeId}"));
        Assert.That(_driver.Url, Does.Contain($"/FoodEntries/Recipes/{_clickedRecipeId}"));
    }
}
