using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using NUnit.Framework;
using MealPlanner.IntegrationTests;


[SetUpFixture]
public class BDDSetup
{
    public static IWebDriver Driver { get; private set; }
    private static string _connectionString;

    [OneTimeSetUp]
    public void Setup()
    {
        try
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionString")
                ?? "Data Source=localhost,1433;Database=MealPlannerDb;User ID=sa;Password=MealPlanner!1234;Pooling=False;Trust Server Certificate=True;Authentication=SqlPassword";
        
            Console.WriteLine("Setting up database...");
            SetupDatabase();
            Console.WriteLine("Database setup complete. Starting AUTHost...");
            AUTHost.Start(_connectionString);
            Console.WriteLine("AUTHost started. Initializing WebDriver...");

            FirefoxOptions options = new FirefoxOptions();
            options.BinaryLocation = "/Applications/Firefox.app/Contents/MacOS/firefox";
            options.AddArgument("--headless");
            options.SetPreference("dom.webdriver.enabled", false);
            options.SetPreference("useAutomationExtension", false);
            Driver = new FirefoxDriver("/opt/homebrew/bin", options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Console.WriteLine("WebDriver initialized successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SETUP FAILED at: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Assert.Fail($"WebDriver initialization failed: {ex.Message}");
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Driver?.Quit();
        Driver?.Dispose();
        AUTHost.Stop();
    }

    public static MealPlannerDBContext CreateContext()
    {
        return new MealPlannerDBContext(
            new DbContextOptionsBuilder<MealPlannerDBContext>()
                .UseSqlServer(_connectionString)
                .Options
        );
    }

    private static void SetupDatabase()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }
}