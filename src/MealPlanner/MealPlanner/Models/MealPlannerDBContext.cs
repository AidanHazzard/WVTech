using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe>(b =>
            {
                b.HasData(
                    new Recipe { Id = -1, Name = "Oatmeal Cookies", Directions = "", Ingredients = "" },
                    new Recipe { Id = -2, Name = "Spaghetti All'assassina", Directions = "", Ingredients = "" },
                    new Recipe { Id = -3, Name = "Spaghetti and Meatballs", Directions = "", Ingredients = "" },
                    new Recipe { Id = -4, Name = "Vegan Spaghetti with Mushrooms", Directions = "", Ingredients = "" },
                    new Recipe { Id = -5, Name = "Baked Spaghetti Casserole", Directions = "", Ingredients = "" },
                    new Recipe { Id = -6, Name = "Mac 'n Cheese Casserole", Directions = "", Ingredients = "" },
                    new Recipe { Id = -7, Name = "Homemade Mac 'n Cheese", Directions = "", Ingredients = "" },
                    new Recipe { Id = -8, Name = "Mushroom Steak Salad", Directions = "", Ingredients = "" },
                    new Recipe { Id = -9, Name = "Ceasar Salad", Directions = "", Ingredients = "" }
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
        }
    }
}
