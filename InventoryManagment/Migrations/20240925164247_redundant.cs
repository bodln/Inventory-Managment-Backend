using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagment.Migrations
{
    /// <inheritdoc />
    public partial class redundant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "LocationHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "LocationHistories");
        }
    }
}
