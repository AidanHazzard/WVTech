using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class AddMealAutoRemovedIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealAutoRemovedIngredient",
                columns: table => new
                {
                    MealId = table.Column<int>(type: "int", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IngredientBaseId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    MeasurementId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealAutoRemovedIngredient", x => new { x.MealId, x.CompletionDate, x.IngredientBaseId });
                    table.ForeignKey(
                        name: "FK_MealAutoRemovedIngredient_IngredientBase_IngredientBaseId",
                        column: x => x.IngredientBaseId,
                        principalTable: "IngredientBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealAutoRemovedIngredient_Meal_MealId",
                        column: x => x.MealId,
                        principalTable: "Meal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealAutoRemovedIngredient_Measurement_MeasurementId",
                        column: x => x.MeasurementId,
                        principalTable: "Measurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealAutoRemovedIngredient_IngredientBaseId",
                table: "MealAutoRemovedIngredient",
                column: "IngredientBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_MealAutoRemovedIngredient_MeasurementId",
                table: "MealAutoRemovedIngredient",
                column: "MeasurementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealAutoRemovedIngredient");
        }
    }
}
