using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class SearchTagSteps
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
    }

    [Then("the tag filter dropdown is visible")]
    public void ThenTheTagFilterDropdownIsVisible()
    {
        var dropdown = _wait.Until(driver =>
        {
            try
            {
                var el = driver.FindElement(By.CssSelector("#tagFilter"));
                return el.Displayed ? el : null;
            }
            catch (NoSuchElementException) { return null; }
        });
        Assert.That(dropdown, Is.Not.Null, "Tag filter dropdown (#tagFilter) was not found on the page");
        Assert.That(dropdown!.Displayed, Is.True);
    }

    [When("{string} selects {string} from the tag filter")]
    public void WhenUserSelectsTagFromFilter(string userName, string tagName)
    {
        // Wait for loadTags() to populate the dropdown option before selecting
        _wait.Until(driver =>
        {
            try
            {
                var select = new SelectElement(driver.FindElement(By.CssSelector("#tagFilter")));
                return select.Options.Any(o => o.GetAttribute("value") == tagName);
            }
            catch { return false; }
        });

        // SelectByValue does not reliably fire jQuery change handlers in headless Chrome.
        // Use jQuery to set the value and trigger the event so recipeSearchHandler runs.
        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "$('#tagFilter').val(arguments[0]).trigger('change');",
            tagName);
    }

    [Then("{string} appears in the recipe search results")]
    public void ThenRecipeAppearsInSearchResults(string recipeName)
    {
        _wait.Until(driver =>
        {
            try
            {
                var names = driver.FindElements(By.CssSelector(".recipeSearchRow .recipeName"));
                return names.Any(el => el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase));
            }
            catch { return false; }
        });

        var names = _driver.FindElements(By.CssSelector(".recipeSearchRow .recipeName"));
        Assert.That(
            names.Any(el => el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)),
            Is.True,
            $"Expected '{recipeName}' in recipe search results but it was not found");
    }

    [Then("{string} does not appear in the recipe search results")]
    public void ThenRecipeDoesNotAppearInSearchResults(string recipeName)
    {
        var names = _driver.FindElements(By.CssSelector(".recipeSearchRow .recipeName"));
        Assert.That(
            names.All(el => !el.Text.Contains(recipeName, StringComparison.OrdinalIgnoreCase)),
            Is.True,
            $"Expected '{recipeName}' NOT to appear in recipe search results but it was found");
    }
}
