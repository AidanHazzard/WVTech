using MealPlanner.DAL.Abstract;
using MealPlanner.DAL.Concrete;
using MealPlanner.Models;
using Microsoft.EntityFrameworkCore; 
using MealPlanner.Services;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Configure the DbContext to use SQL Server
string connectionString = builder.Configuration["ConnectionString"];

builder.Services.AddDbContext<MealPlannerDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserDietaryRestrictionRepository, UserDietaryRestrictionRepository>();

builder.Services.AddScoped<INutritionProgressService, NutritionProgressService>();

var app = builder.Build();

await SeedService.SeedData(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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