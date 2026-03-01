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
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<MealPlannerDBContext>()
.AddDefaultTokenProviders();

//when an unauthorized user tries to access a protected resource, redirect them to the login page
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";           // must match your LoginController
    options.AccessDeniedPath = "/Login";    // optional
});

builder.Services.AddScoped<INutritionProgressService, NutritionProgressService>();
// Register LoginService for dependency injection
builder.Services.AddScoped<ILoginService, LoginService>();
// Register RegisterService for dependency injection
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
// Register AccountSettingsService for dependency injection
builder.Services.AddScoped<IAccountSettingsService, AccountSettingsService>();


// Register EmailService for dependency injection
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

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
