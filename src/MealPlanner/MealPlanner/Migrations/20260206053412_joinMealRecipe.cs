using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class joinMealRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_Meal_MealId",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "IX_Recipe_MealId",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "MealId",
                table: "Recipe");

            migrationBuilder.CreateTable(
                name: "MealRecipe",
                columns: table => new
                {
                    MealsId = table.Column<int>(type: "int", nullable: false),
                    RecipesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealRecipe", x => new { x.MealsId, x.RecipesId });
                    table.ForeignKey(
                        name: "FK_MealRecipe_Meal_MealsId",
                        column: x => x.MealsId,
                        principalTable: "Meal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealRecipe_Recipe_RecipesId",
                        column: x => x.RecipesId,
                        principalTable: "Recipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealRecipe_RecipesId",
                table: "MealRecipe",
                column: "RecipesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealRecipe");

            migrationBuilder.AddColumn<int>(
                name: "MealId",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_MealId",
                table: "Recipe",
                column: "MealId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_Meal_MealId",
                table: "Recipe",
                column: "MealId",
                principalTable: "Meal",
                principalColumn: "Id");
        }
    }
}
