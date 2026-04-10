using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

[Binding]
public class SharedDriver
{
    public IWebDriver Driver { get; private set; } = null!;
    public WebDriverWait Wait { get; private set; } = null!;
    public const string BaseUrl = "https://localhost:57049";

    [BeforeScenario]
    public void SetUp()
    {
        ChromeOptions options = new ChromeOptions();
        // options.AddArgument("--headless");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--ignore-certificate-errors");

        Driver = new ChromeDriver(options);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
    }

    [AfterScenario]
    public void TearDown()
    {
        Driver?.Quit();
    }
}