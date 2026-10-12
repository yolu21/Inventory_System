using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySys.Migrations
{
    /// <inheritdoc />
    public partial class FixIngredientForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_StockRecords_IngredientId",
                table: "StockRecords",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_Name",
                table: "Ingredients",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockRecords_Ingredients_IngredientId",
                table: "StockRecords",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockRecords_Ingredients_IngredientId",
                table: "StockRecords");

            migrationBuilder.DropIndex(
                name: "IX_StockRecords_IngredientId",
                table: "StockRecords");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_Name",
                table: "Ingredients");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
