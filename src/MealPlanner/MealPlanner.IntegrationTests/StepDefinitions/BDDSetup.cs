using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace MealPlanner.IntegrationTests;

[SetUpFixture]
public class BDDSetup
{
    public static IWebDriver Driver { get; private set; }
    private static string _connectionString;

    [OneTimeSetUp]
    public void Setup()
    {
        _connectionString = Environment.GetEnvironmentVariable("ConnectionString")
            ?? "Data Source=localhost,1433;Database=MealPlannerDb;User ID=sa;Password=MealPlanner!1234;Pooling=False;Trust Server Certificate=True;Authentication=SqlPassword";

        SetupDatabase();
        AUTHost.Start(_connectionString);

        FirefoxOptions options = new FirefoxOptions();
        options.AddArgument("--headless");

        Driver = new FirefoxDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
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