using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using System.Threading;

namespace Mealplanner.IntegrationTests;

[Binding]
public class WVT22Steps
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly WebDriverWait _wait;

    public WVT22Steps()
    {
        _driver = BDDSetup.Driver;
        _baseUrl = AUTHost.BaseUrl;
        _wait = BDDSetup.Wait;
    }

    [Given("'Dave' is on the shopping list page")]
    public void GivenDaveIsOnShoppingListPage()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/ShoppingList");
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }

    [Then("a zip code input field is visible")]
    public void ThenAZipCodeInputFieldIsVisible()
    {
        var input = _wait.Until(d =>
        {
            var el = d.FindElements(By.Id("ZipCode")).FirstOrDefault();
            return el != null && el.Displayed ? el : null;
        });
        Assert.That(input, Is.Not.Null, "Expected a visible zip code input with id='ZipCode'");
    }

    [When("'Dave' enters zip code {string} and clicks export to Kroger")]
    public void WhenDaveEntersZipCodeAndClicksExport(string zipCode)
    {
        var input = _wait.Until(d => d.FindElement(By.Id("ZipCode")));
        input.Clear();
        input.SendKeys(zipCode);

        // Click Find Stores to trigger the store lookup
        var findBtn = _wait.Until(d => d.FindElement(By.Id("findKrogerStores")));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", findBtn);
        findBtn.Click();

        // Wait for the store section to appear (stores found or no-stores message)
        _wait.Until(d =>
        {
            var section = d.FindElements(By.Id("krogerStoreSection")).FirstOrDefault();
            return section != null && section.Displayed;
        });

        // Give SaveZip POST time to complete before navigating away
        System.Threading.Thread.Sleep(500);

        // Click Export if the button is available (stores were found)
        var exportBtn = _driver.FindElements(By.Id("exportToKroger"))
            .FirstOrDefault(e => e.Displayed);

        if (exportBtn != null)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", exportBtn);
            exportBtn.Click();

            _wait.Until(d => ((IJavaScriptExecutor)d)
                .ExecuteScript("return document.readyState")?.ToString() == "complete");
        }

        // Always return to shopping list so the zip can be verified
        if (!_driver.Url.Contains("ShoppingList"))
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/ShoppingList");
            _wait.Until(d => ((IJavaScriptExecutor)d)
                .ExecuteScript("return document.readyState")?.ToString() == "complete");
        }
    }

    [Then("the zip code {string} is shown in the export section")]
    public void ThenTheZipCodeIsShownInExportSection(string zipCode)
    {
        var input = _wait.Until(d => d.FindElement(By.Id("ZipCode")));
        Assert.That(input.GetAttribute("value"), Is.EqualTo(zipCode),
            $"Expected zip code input to show '{zipCode}'");
    }

    [When("'Dave' navigates away and returns to the shopping list page")]
    public void WhenDaveNavigatesAwayAndReturnsToShoppingList()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Meal/PlannerHome");
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");

        _driver.Navigate().GoToUrl($"{_baseUrl}/ShoppingList");
        _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").ToString() == "complete");
    }
}
