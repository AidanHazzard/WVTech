using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class updateSeedRecipeCalories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -9,
                column: "Calories",
                value: 300);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -8,
                column: "Calories",
                value: 400);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -7,
                column: "Calories",
                value: 850);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -6,
                column: "Calories",
                value: 550);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -5,
                column: "Calories",
                value: 400);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -4,
                column: "Calories",
                value: 350);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -3,
                column: "Calories",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -2,
                column: "Calories",
                value: 400);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -1,
                column: "Calories",
                value: 250);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -9,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -8,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -7,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -6,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -5,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -4,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -3,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -2,
                column: "Calories",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -1,
                column: "Calories",
                value: 0);
        }
    }
}
