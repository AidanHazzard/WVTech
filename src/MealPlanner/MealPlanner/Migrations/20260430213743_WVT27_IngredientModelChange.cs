using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Migrations
{
    /// <inheritdoc />
    public partial class WVT27_IngredientModelChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Measurement",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ShoppingListItems");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ShoppingListItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IngredientBaseId",
                table: "ShoppingListItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MeasurementId",
                table: "ShoppingListItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Ingredient",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            // Backfill DisplayName from IngredientBase.Name for existing ingredient rows.
            migrationBuilder.Sql(@"
                UPDATE i SET i.DisplayName = ib.Name
                FROM Ingredient i
                JOIN IngredientBase ib ON i.IngredientBaseId = ib.Id
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_IngredientBaseId",
                table: "ShoppingListItems",
                column: "IngredientBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_MeasurementId",
                table: "ShoppingListItems",
                column: "MeasurementId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListItems_IngredientBase_IngredientBaseId",
                table: "ShoppingListItems",
                column: "IngredientBaseId",
                principalTable: "IngredientBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListItems_Measurement_MeasurementId",
                table: "ShoppingListItems",
                column: "MeasurementId",
                principalTable: "Measurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListItems_IngredientBase_IngredientBaseId",
                table: "ShoppingListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListItems_Measurement_MeasurementId",
                table: "ShoppingListItems");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingListItems_IngredientBaseId",
                table: "ShoppingListItems");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingListItems_MeasurementId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "IngredientBaseId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "MeasurementId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Ingredient");

            migrationBuilder.AddColumn<string>(
                name: "Measurement",
                table: "ShoppingListItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ShoppingListItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
