using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;
using MealPlanner.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ThemeFilter>();
});

// Get Secrets from Azure Key Vault (production only)
if (builder.Environment.IsProduction())
{
    var keyVaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        SecretClient secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        builder.Configuration["EmailSettings:Password"] =
            secretClient.GetSecret("EmailSettings--Password").Value.Value;
        builder.Configuration["Edamam:AppId"] = secretClient.GetSecret("Edamam--AppId").Value.Value;
        builder.Configuration["Edamam:ApiKey"] = secretClient.GetSecret("Edamam--ApiKey").Value.Value;
    }
}

// Create db context
// Create connection string
string? connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionString"];
string databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
// Debug output
Console.WriteLine("USING CONNECTION STRING: " + connectionString);

// Validate connection string
if (string.IsNullOrWhiteSpace(connectionString) && databaseProvider == "SqlServer")
{
    throw new InvalidOperationException(
        "Missing connection string. Set 'ConnectionStrings:DefaultConnection' or 'ConnectionString'."
    );
}

builder.Services.AddDbContext<MealPlannerDBContext>(options =>
    options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure()));

builder.Services.AddScoped<DbContext, MealPlannerDBContext>();

// Add Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserDietaryRestrictionRepository, UserDietaryRestrictionRepository>();
builder.Services.AddScoped<IMealRepository, MealRepository>();
builder.Services.AddScoped<IUserRecipeRepository, UserRecipeRepository>();
builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.AddScoped<ThemeFilter>();
builder.Services.AddScoped<ShoppingListService>();

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<MealPlannerDBContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

//when an unauthorized user tries to access a protected resource, redirect them to the login page
builder.Services.ConfigureApplicationCookie(options =>
{
    Console.WriteLine("Auth Cookie Configuration Loaded");
    options.LoginPath = "/Login";           // must match your LoginController
    options.AccessDeniedPath = "/Login";    // optional

     // Sliding expiration okay
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;

    options.ExpireTimeSpan = TimeSpan.FromDays(30);

    options.Cookie.IsEssential = true;
});

// Add services
builder.Services.AddScoped<INutritionProgressService, NutritionProgressService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();

// External APIs
if (builder.Configuration["NoApi"] != "true")
{
    
    string edamamAppId = builder.Configuration.GetSection("Edamam")["AppId"];
    string edamamAPIKey = builder.Configuration.GetSection("Edamam")["ApiKey"];
    string edamamAPIUrl = "https://api.edamam.com/api/";
    builder.Services.AddHttpClient<IExternalRecipeService, EdamamService>(httpClient =>
    {
        httpClient.BaseAddress = new Uri(edamamAPIUrl);
        httpClient.DefaultRequestHeaders.Add("accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "en");
        return new EdamamService(httpClient, edamamAppId, edamamAPIKey);
    });
}

// Configure emailer
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Use static assets for staging
if (builder.Environment.IsStaging())
{
    builder.WebHost.UseStaticWebAssets();
}

var app = builder.Build();

await SeedService.SeedData(app.Services);

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();