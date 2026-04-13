using System;
using Mealplanner.IntegrationTests;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using MealPlanner.IntegrationTests;

[Binding]
public class SharedDriver
{
    public IWebDriver Driver => BDDSetup.Driver;
    public WebDriverWait Wait { get; private set; } = null!;
    public string BaseUrl => AUTHost.BaseUrl;

    [BeforeScenario]
    public void SetUp()
    {
        if (BDDSetup.Driver == null)
            throw new Exception("BDDSetup.Driver is null. OneTimeSetUp did not initialize the WebDriver.");

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
    }
}