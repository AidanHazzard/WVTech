using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using MealPlanner.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// ✅ Configure the DbContext to use SQL Server
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? builder.Configuration["ConnectionString"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing connection string. Set user-secrets 'ConnectionStrings:DefaultConnection' or 'ConnectionString'."
    );
}

builder.Services.AddDbContext<MealPlannerDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<DbContext, MealPlannerDBContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserDietaryRestrictionRepository, UserDietaryRestrictionRepository>();

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

// Register AccountService for dependency injection
builder.Services.AddScoped<IAccountService, AccountService>();

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
