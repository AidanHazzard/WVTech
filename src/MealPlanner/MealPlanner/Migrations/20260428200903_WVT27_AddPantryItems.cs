using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class WVT27_AddPantryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Ingredient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredient_UserId",
                table: "Ingredient",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredient_AspNetUsers_UserId",
                table: "Ingredient",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredient_AspNetUsers_UserId",
                table: "Ingredient");

            migrationBuilder.DropIndex(
                name: "IX_Ingredient_UserId",
                table: "Ingredient");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Ingredient");
        }
    }
}
