using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MealPlanner.Models
{
    public class MealPlannerDBContext : IdentityDbContext<User>
    {
        public MealPlannerDBContext(DbContextOptions<MealPlannerDBContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserNutritionPreference> UserNutritionPreferences { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<DietaryRestriction> DietaryRestrictions { get; set; }
        public DbSet<UserDietaryRestriction> UserDietaryRestrictions { get; set; }

        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe>(b =>
            {
                b.HasData(
                    new Recipe { Id = -1, Name = "Oatmeal Cookies", Directions = "" },
                    new Recipe { Id = -2, Name = "Spaghetti All'assassina", Directions = "" },
                    new Recipe { Id = -3, Name = "Spaghetti and Meatballs", Directions = "" },
                    new Recipe { Id = -4, Name = "Vegan Spaghetti with Mushrooms", Directions = "" },
                    new Recipe { Id = -5, Name = "Baked Spaghetti Casserole", Directions = "" },
                    new Recipe { Id = -6, Name = "Mac 'n Cheese Casserole", Directions = "" },
                    new Recipe { Id = -7, Name = "Homemade Mac 'n Cheese", Directions = "" },
                    new Recipe { Id = -8, Name = "Mushroom Steak Salad", Directions = "" },
                    new Recipe { Id = -9, Name = "Ceasar Salad", Directions = "" }
                );
            });

            modelBuilder.Entity<UserDietaryRestriction>()
                .HasKey(udr => new { udr.UserId, udr.DietaryRestrictionId });

            modelBuilder.Entity<UserDietaryRestriction>()
                .HasOne(udr => udr.User)
                .WithMany()
                .HasForeignKey(udr => udr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDietaryRestriction>()
                .HasOne(udr => udr.DietaryRestriction)
                .WithMany()
                .HasForeignKey(udr => udr.DietaryRestrictionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ingredient>()
                .Navigation(i => i.IngredientBase)
                .AutoInclude();

            modelBuilder.Entity<Ingredient>()
                .Navigation(i => i.Measurement)
                .AutoInclude();
        }
    }
}