using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class recipeSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Recipe",
                columns: new[] { "Id", "Directions", "Ingredients", "Name" },
                values: new object[,]
                {
                    { -9, "", "", "Ceasar Salad" },
                    { -8, "", "", "Mushroom Steak Salad" },
                    { -7, "", "", "Homemade Mac 'n Cheese" },
                    { -6, "", "", "Mac 'n Cheese Casserole" },
                    { -5, "", "", "Baked Spaghetti Casserole" },
                    { -4, "", "", "Vegan Spaghetti with Mushrooms" },
                    { -3, "", "", "Spaghetti and Meatballs" },
                    { -2, "", "", "Spaghetti All'assassina" },
                    { -1, "", "", "Oatmeal Cookies" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -9);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -8);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -7);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -6);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -5);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -1);
        }
    }
}
