using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MealPlanner.IntegrationTests;

[SetUpFixture]
public class BDDSetup
{
    public TestContext? TestContext { get; set; }
    public static IWebDriver Driver { get; private set; }
    public static WebDriverWait Wait { get; private set; }
    public static MealPlannerDBContext Context { get; private set; }
    private static string _connectionString;

    [OneTimeSetUp]
    public void Setup()
    {
        try
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionString")
                ?? TestContext?.Parameters["ConnectionString"]
                ?? "Data Source=localhost,1433;Database=MealPlannerDb;User ID=sa;Password=MealPlanner!1234;Pooling=False;Trust Server Certificate=True;Authentication=SqlPassword";

            Console.WriteLine("Setting up database...");
            SetupDatabase();
            Console.WriteLine("Database setup complete. Starting AUTHost...");
            AUTHost.Start(_connectionString);
            Console.WriteLine("AUTHost started. Initializing WebDriver...");

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--window-size=2560,1440");
            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
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
        Context?.Dispose();
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
        Context = CreateContext();
        Context.Database.EnsureDeleted();
    }
}