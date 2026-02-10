using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Models;

namespace MealPlanner.Models
{
    public class MealPlannerDBContext : IdentityDbContext<UserAccount>
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

