using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Models
{
    public class MealPlannerDBContext : DbContext
    {
        public MealPlannerDBContext(DbContextOptions<MealPlannerDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserNutritionPreference> UserNutritionPreferences { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<UserDietaryRestriction> UserDietaryRestrictions { get; set; }
    }
}

