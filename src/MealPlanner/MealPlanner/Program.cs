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
// Create connection string
string? connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionString"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing connection string. Set 'ConnectionStrings:DefaultConnection' or 'ConnectionString'."
    );
}

// Get Secrets from Azure Key Vault (production only)
if (builder.Environment.IsProduction())
{
    var keyVaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        SecretClient secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        builder.Configuration["EmailSettings:Password"] =
            secretClient.GetSecret("EmailSettings--Password").Value.Value;
    }
}

// Create db context
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
builder.Services.AddScoped<ThemeFilter>(); // add this

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
builder.Services.AddScoped<IAccountSettingsService, AccountSettingsService>();

// Configure emailer
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Register EmailService for dependency injection
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

await SeedService.SeedData(app.Services);

if (!app.Environment.IsDevelopment())
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