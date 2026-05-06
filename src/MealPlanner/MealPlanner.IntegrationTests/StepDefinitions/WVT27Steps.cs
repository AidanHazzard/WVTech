using MealPlanner.Models;
using MealPlanner.Services;
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
        _driver.Navigate().GoToUrl($"{_baseUrl}/Shopping/Pantry");
        _wait.Until(d => d.Url.Contains("/Pantry", StringComparison.OrdinalIgnoreCase));
    }

    [When("{string} submits the add pantry item form with name {string}, amount {string}, and measurement {string}")]
    public void WhenUserSubmitsAddPantryItemForm(string userName, string name, string amount, string measurement)
    {
        _driver.FindElement(By.Id("Amount")).Clear();
        _driver.FindElement(By.Id("Amount")).SendKeys(amount);
        new SelectElement(_driver.FindElement(By.Id("Measurement"))).SelectByText(measurement);
        _driver.FindElement(By.Id("Name")).Clear();
        _driver.FindElement(By.Id("Name")).SendKeys(name);
        _driver.FindElement(By.Id("addPantryItemBtn")).Click();
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [When("{string} submits the add pantry item form with no name, amount {string}, and measurement {string}")]
    public void WhenUserSubmitsAddPantryItemFormNoName(string userName, string amount, string measurement)
    {
        _itemCountBeforeSubmit = _driver.FindElements(By.CssSelector(".pantry-item")).Count;
        _driver.FindElement(By.Id("Amount")).Clear();
        _driver.FindElement(By.Id("Amount")).SendKeys(amount);
        new SelectElement(_driver.FindElement(By.Id("Measurement"))).SelectByText(measurement);
        _driver.FindElement(By.Id("Name")).Clear();
        _driver.FindElement(By.Id("addPantryItemBtn")).Click();
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("{string} appears in the pantry list")]
    public void ThenItemAppearsInPantryList(string name)
    {
        var display = DisplayName(name);
        var items = _driver.FindElements(By.CssSelector(".pantry-item-name"));
        Assert.That(
            items.Any(el => el.Text.Contains(display, StringComparison.OrdinalIgnoreCase)),
            Is.True,
            $"Expected '{name}' in pantry list but it was not found");
    }

    [Then("the pantry list shows {string} with an amount of {string} and measurement {string}")]
    public void ThenPantryListShowsItemWithDetails(string name, string amount, string measurement)
    {
        var display = DisplayName(name);
        var items = _driver.FindElements(By.CssSelector(".pantry-item"));
        var match = items.FirstOrDefault(el =>
            el.Text.Contains(display, StringComparison.OrdinalIgnoreCase));
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

    [Given("{string} has no pantry items")]
    public void GivenUserHasNoPantryItems(string userName)
    {
        var email = $"{userName}@fakeemail.com";
        var pantryItems = BDDSetup.Context.Set<User>()
            .Where(u => u.Email == email)
            .SelectMany(u => u.PantryItems)
            .ToList();
        if (pantryItems.Any())
        {
            BDDSetup.Context.Set<Ingredient>().RemoveRange(pantryItems);
            BDDSetup.Context.SaveChanges();
        }
        _driver.Navigate().GoToUrl($"{_baseUrl}/Shopping/Pantry");
        _wait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Given("{string} has a pantry item named {string} with amount {string} and measurement {string}")]
    public void GivenUserHasPantryItem(string userName, string name, string amount, string measurement)
    {
        _driver.FindElement(By.Id("Amount")).Clear();
        _driver.FindElement(By.Id("Amount")).SendKeys(amount);
        new SelectElement(_driver.FindElement(By.Id("Measurement"))).SelectByText(measurement);
        _driver.FindElement(By.Id("Name")).Clear();
        _driver.FindElement(By.Id("Name")).SendKeys(name);
        _driver.FindElement(By.Id("addPantryItemBtn")).Click();
        var displayName = DisplayName(name);
        _wait.Until(d =>
        {
            var items = d.FindElements(By.CssSelector(".pantry-item"));
            return items.Any(el => el.Text.Contains(displayName, StringComparison.OrdinalIgnoreCase));
        });
    }

    [When("{string} removes the pantry item named {string}")]
    public void WhenUserRemovesPantryItem(string userName, string name)
    {
        var display = DisplayName(name);
        var items = _driver.FindElements(By.CssSelector(".pantry-item"));
        var row = items.FirstOrDefault(el => el.Text.Contains(display, StringComparison.OrdinalIgnoreCase));
        Assert.That(row, Is.Not.Null, $"Could not find pantry item '{name}' to remove");
        row!.FindElement(By.CssSelector(".remove-pantry-item")).Click();
        _wait.Until(d => d.Url.Contains("/Pantry", StringComparison.OrdinalIgnoreCase));
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("the pantry list is empty")]
    public void ThenPantryListIsEmpty()
    {
        var items = _driver.FindElements(By.CssSelector(".pantry-item"));
        Assert.That(items.Count, Is.EqualTo(0),
            $"Expected pantry to be empty but found {items.Count} item(s)");
    }

    [Then("{string} does not appear in the pantry list")]
    public void ThenItemDoesNotAppearInPantryList(string name)
    {
        var display = DisplayName(name);
        var items = _driver.FindElements(By.CssSelector(".pantry-item-name"));
        Assert.That(
            items.Any(el => el.Text.Contains(display, StringComparison.OrdinalIgnoreCase)),
            Is.False,
            $"Expected '{name}' to be absent from pantry list but it was found");
    }

    private static string DisplayName(string name) =>
        IngredientNameNormalizer.Normalize(name);

    private IWebElement FindPantryRow(string name)
    {
        var display = DisplayName(name);
        IWebElement? row = null;
        _wait.Until(d =>
        {
            var rows = d.FindElements(By.CssSelector(".pantry-item"));
            row = rows.FirstOrDefault(el => el.Text.Contains(display, StringComparison.OrdinalIgnoreCase));
            return row != null;
        });
        return row!;
    }

    [When("{string} updates the amount of {string} to {string}")]
    public void WhenUserUpdatesAmount(string userName, string name, string newAmount)
    {
        var row = FindPantryRow(name);
        var input = row.FindElement(By.CssSelector(".pantry-qty-input"));
        input.Clear();
        input.SendKeys(newAmount);
        row.FindElement(By.CssSelector(".pantry-qty-save")).Click();
        _wait.Until(d => d.Url.Contains("/Pantry", StringComparison.OrdinalIgnoreCase));
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [When("{string} updates the amount of {string} to {string} via javascript")]
    public void WhenUserUpdatesAmountViaJavascript(string userName, string name, string newAmount)
    {
        var row = FindPantryRow(name);
        var input = row.FindElement(By.CssSelector(".pantry-qty-input"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].value = arguments[1]", input, newAmount);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].form.submit()", input);
        _wait.Until(d => d.Url.Contains("/Pantry", StringComparison.OrdinalIgnoreCase));
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("the pantry shows {string} with an amount of {string}")]
    [Then("{string} still shows an amount of {string}")]
    public void ThenPantryShowsItemWithAmount(string name, string amount)
    {
        var display = DisplayName(name);
        var rows = _driver.FindElements(By.CssSelector(".pantry-item"));
        var row = rows.FirstOrDefault(el => el.Text.Contains(display, StringComparison.OrdinalIgnoreCase));
        Assert.That(row, Is.Not.Null, $"Expected '{name}' in pantry list but it was not found");
        var input = row!.FindElement(By.CssSelector(".pantry-qty-input"));
        Assert.That(input.GetAttribute("value"), Is.EqualTo(amount),
            $"Expected '{name}' to have amount '{amount}' but was '{input.GetAttribute("value")}'");
    }
}
