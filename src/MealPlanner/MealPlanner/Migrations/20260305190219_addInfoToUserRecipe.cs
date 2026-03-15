using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class addInfoToUserRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "RecipeUser",
                columns: table => new
                {
                    RecipesId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeUser", x => new { x.RecipesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RecipeUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeUser_Recipe_RecipesId",
                        column: x => x.RecipesId,
                        principalTable: "Recipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRecipe",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    UserFavorite = table.Column<bool>(type: "bit", nullable: false),
                    UserOwner = table.Column<bool>(type: "bit", nullable: false),
                    UserVote = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecipe", x => new { x.UserId, x.RecipeId });
                    table.ForeignKey(
                        name: "FK_UserRecipe_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRecipe_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeUser_UsersId",
                table: "RecipeUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecipe_RecipeId",
                table: "UserRecipe",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeUser");

            migrationBuilder.DropTable(
                name: "UserRecipe");

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
    }
}
