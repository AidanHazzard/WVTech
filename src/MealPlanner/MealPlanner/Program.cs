using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionString"]
    ?? throw new InvalidOperationException("Missing connection string. Set user-secrets 'ConnectionStrings:DefaultConnection' or 'ConnectionString'.");

Console.WriteLine("USING CONNECTION STRING: " + connectionString);

builder.Services.AddDbContext<MealPlannerDBContext>(options =>
    options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure()));

builder.Services.AddScoped<DbContext, MealPlannerDBContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserDietaryRestrictionRepository, UserDietaryRestrictionRepository>();
builder.Services.AddScoped<IMealRepository, MealRepository>();
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<MealPlannerDBContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<INutritionProgressService, NutritionProgressService>();
// Register LoginService for dependency injection
builder.Services.AddScoped<ILoginService, LoginService>();
// Register RegisterService for dependency injection
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
// Register AccountSettingsService for dependency injection
builder.Services.AddScoped<IAccountSettingsService, AccountSettingsService>();


var app = builder.Build();

await SeedService.SeedData(app.Services);

// Configure the HTTP request pipeline.
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
