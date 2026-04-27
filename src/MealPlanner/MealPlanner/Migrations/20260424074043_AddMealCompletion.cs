using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class AddMealCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealCompletion",
                columns: table => new
                {
                    MealId = table.Column<int>(type: "int", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealCompletion", x => new { x.MealId, x.CompletionDate });
                    table.ForeignKey(
                        name: "FK_MealCompletion_Meal_MealId",
                        column: x => x.MealId,
                        principalTable: "Meal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealCompletion");
        }
    }
}
