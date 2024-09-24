using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagment.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillOfSaleItem");

            migrationBuilder.DropTable(
                name: "ItemShipmentOrder");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "ShipmentOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ShipmentOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "BillsOfSale",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "BillsOfSale",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentOrders_ItemId",
                table: "ShipmentOrders",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfSale_ItemId",
                table: "BillsOfSale",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillsOfSale_Items_ItemId",
                table: "BillsOfSale",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentOrders_Items_ItemId",
                table: "ShipmentOrders",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillsOfSale_Items_ItemId",
                table: "BillsOfSale");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentOrders_Items_ItemId",
                table: "ShipmentOrders");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentOrders_ItemId",
                table: "ShipmentOrders");

            migrationBuilder.DropIndex(
                name: "IX_BillsOfSale_ItemId",
                table: "BillsOfSale");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "ShipmentOrders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ShipmentOrders");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "BillsOfSale");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "BillsOfSale");

            migrationBuilder.CreateTable(
                name: "BillOfSaleItem",
                columns: table => new
                {
                    BillOfSalesId = table.Column<int>(type: "int", nullable: false),
                    ItemsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfSaleItem", x => new { x.BillOfSalesId, x.ItemsId });
                    table.ForeignKey(
                        name: "FK_BillOfSaleItem_BillsOfSale_BillOfSalesId",
                        column: x => x.BillOfSalesId,
                        principalTable: "BillsOfSale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillOfSaleItem_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemShipmentOrder",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    ShipmentOrdersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemShipmentOrder", x => new { x.ItemsId, x.ShipmentOrdersId });
                    table.ForeignKey(
                        name: "FK_ItemShipmentOrder_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemShipmentOrder_ShipmentOrders_ShipmentOrdersId",
                        column: x => x.ShipmentOrdersId,
                        principalTable: "ShipmentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillOfSaleItem_ItemsId",
                table: "BillOfSaleItem",
                column: "ItemsId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemShipmentOrder_ShipmentOrdersId",
                table: "ItemShipmentOrder",
                column: "ShipmentOrdersId");
        }
    }
}
