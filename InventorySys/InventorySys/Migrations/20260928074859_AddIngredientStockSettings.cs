using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySys.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientStockSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinimumStock",
                table: "Ingredients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "Ingredients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumStock",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "Ingredients");
        }
    }
}
