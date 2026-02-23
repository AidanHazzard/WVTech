using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeMacros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Calories",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarbGrams",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FatGrams",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProteinGrams",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -9,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -8,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -7,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -6,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -5,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -4,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -3,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -2,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "Calories", "CarbGrams", "FatGrams", "ProteinGrams" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Calories",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "CarbGrams",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "FatGrams",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "ProteinGrams",
                table: "Recipe");
        }
    }
}
