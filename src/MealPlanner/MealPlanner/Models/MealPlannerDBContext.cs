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


        // Add other DbSets for your models here
        //      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        //         => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=MealPlannerDb;User Id=sa;Password=Axts-mv135!;");

    }
}

