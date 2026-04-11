using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Mealplanner.IntegrationTests;

[SetUpFixture]
public class BDDSetup
{
    public static IWebDriver Driver { get; private set;}
    private static string _connectionString;

    [OneTimeSetUp]
    public void Setup()
    {
        _connectionString = Environment.GetEnvironmentVariable("ConnectionString")
            ?? "Data Source=localhost,1434;Database=OnebiteTest;User ID=SA;Password=1234TestP@ssword;Pooling=False;Trust Server Certificate=True;Authentication=SqlPassword";
        SetupDatabase();
        AUTHost.Start(_connectionString);

        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");


        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Driver.Quit();
        Driver.Dispose();
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