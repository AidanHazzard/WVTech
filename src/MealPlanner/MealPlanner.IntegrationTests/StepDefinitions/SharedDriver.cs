using System;
using Mealplanner.IntegrationTests;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

[Binding]
public class SharedDriver
{
    public IWebDriver Driver => BDDSetup.Driver;
    public WebDriverWait Wait { get; private set; } = null!;
    public string BaseUrl => AUTHost.BaseUrl;

    [BeforeScenario]
    public void SetUp()
    {
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
    }
}