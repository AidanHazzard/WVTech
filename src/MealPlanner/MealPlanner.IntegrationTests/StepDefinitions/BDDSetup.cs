using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Mealplanner.IntegrationTests;

[SetUpFixture]
public class BDDSetup
{
    public TestContext? TestContext { get; set; }
    public static IWebDriver Driver { get; private set;}
    public static WebDriverWait Wait { get; private set; }
    public static MealPlannerDBContext Context { get; private set; }
    private static string _connectionString;

    [OneTimeSetUp]
    public void Setup()
    {
        _connectionString = Environment.GetEnvironmentVariable("ConnectionString")
            ?? TestContext.Parameters["ConnectionString"]
            ?? throw new InvalidOperationException("Connection String not provided");

        SetupDatabase();
        AUTHost.Start(_connectionString);
        
        //FirefoxOptions options = new FirefoxOptions();
        ChromeOptions options = new ChromeOptions();
        // options.AddArgument("--headless");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--window-size=2560,1440");


        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Driver.Quit();
        Driver.Dispose();
        AUTHost.Stop();
        Context.Dispose();
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