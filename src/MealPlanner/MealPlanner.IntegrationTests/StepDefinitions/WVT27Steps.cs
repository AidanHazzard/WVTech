using MealPlanner.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT27Steps
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;
    private string _baseUrl = null!;

    private int _itemCountBeforeSubmit;

    [BeforeScenario]
    public void SetUp()
    {
        _driver = BDDSetup.Driver;
        _wait = BDDSetup.Wait;
        _baseUrl = AUTHost.BaseUrl;
    }

    [Given("{string} is on the pantry page")]
    public void GivenUserIsOnThePantryPage(string userName)
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Pantry");
        _wait.Until(d => d.Url.Contains("/Pantry", StringComparison.OrdinalIgnoreCase));
    }

    [When("{string} submits the add pantry item form with name {string}, amount {string}, and measurement {string}")]
    public void WhenUserSubmitsAddPantryItemForm(string userName, string name, string amount, string measurement)
    {
        _driver.FindElement(By.Id("Name")).Clear();
        _driver.FindElement(By.Id("Name")).SendKeys(name);
        _driver.FindElement(By.Id("Amount")).Clear();
        _driver.FindElement(By.Id("Amount")).SendKeys(amount);
        _driver.FindElement(By.Id("Measurement")).Clear();
        _driver.FindElement(By.Id("Measurement")).SendKeys(measurement);
        _driver.FindElement(By.Id("addPantryItemBtn")).Click();
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [When("{string} submits the add pantry item form with no name, amount {string}, and measurement {string}")]
    public void WhenUserSubmitsAddPantryItemFormNoName(string userName, string amount, string measurement)
    {
        _itemCountBeforeSubmit = _driver.FindElements(By.CssSelector(".pantry-item")).Count;
        _driver.FindElement(By.Id("Name")).Clear();
        _driver.FindElement(By.Id("Amount")).Clear();
        _driver.FindElement(By.Id("Amount")).SendKeys(amount);
        _driver.FindElement(By.Id("Measurement")).Clear();
        _driver.FindElement(By.Id("Measurement")).SendKeys(measurement);
        _driver.FindElement(By.Id("addPantryItemBtn")).Click();
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("{string} appears in the pantry list")]
    public void ThenItemAppearsInPantryList(string name)
    {
        var items = _driver.FindElements(By.CssSelector(".pantry-item-name"));
        Assert.That(
            items.Any(el => el.Text.Contains(name, StringComparison.OrdinalIgnoreCase)),
            Is.True,
            $"Expected '{name}' in pantry list but it was not found");
    }

    [Then("the pantry list shows {string} with an amount of {string} and measurement {string}")]
    public void ThenPantryListShowsItemWithDetails(string name, string amount, string measurement)
    {
        var items = _driver.FindElements(By.CssSelector(".pantry-item"));
        var match = items.FirstOrDefault(el =>
            el.Text.Contains(name, StringComparison.OrdinalIgnoreCase));
        Assert.That(match, Is.Not.Null, $"Expected '{name}' in pantry list but it was not found");
        Assert.That(match!.Text, Does.Contain(amount));
        Assert.That(match.Text, Does.Contain(measurement).IgnoreCase);
    }

    [Then("a success message is displayed on the pantry page")]
    public void ThenSuccessMessageIsDisplayed()
    {
        var msg = _wait.Until(d =>
        {
            var el = d.FindElements(By.CssSelector(".alert-success")).FirstOrDefault();
            return el != null && el.Displayed ? el : null;
        });
        Assert.That(msg, Is.Not.Null, "Expected a success message to be displayed");
    }

    [Then("an error message is displayed on the pantry page")]
    public void ThenErrorMessageIsDisplayed()
    {
        var el = _driver.FindElements(By.CssSelector(".field-validation-error, .text-danger"))
                        .FirstOrDefault(e => e.Displayed);
        Assert.That(el, Is.Not.Null, "Expected a validation error message to be displayed");
    }

    [Then("no new item is added to the pantry list")]
    public void ThenNoNewItemIsAddedToPantryList()
    {
        var items = _driver.FindElements(By.CssSelector(".pantry-item"));
        Assert.That(items.Count, Is.EqualTo(_itemCountBeforeSubmit),
            $"Expected {_itemCountBeforeSubmit} pantry items but found {items.Count}");
    }
}
