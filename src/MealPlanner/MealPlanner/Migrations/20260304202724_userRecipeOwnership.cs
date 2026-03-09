using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class userRecipeOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Recipe",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -9,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -8,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -7,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -6,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -5,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -4,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -3,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -2,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Recipe",
                keyColumn: "Id",
                keyValue: -1,
                column: "OwnerId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_OwnerId",
                table: "Recipe",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_AspNetUsers_OwnerId",
                table: "Recipe",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_AspNetUsers_OwnerId",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "IX_Recipe_OwnerId",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Recipe");
        }
    }
}
