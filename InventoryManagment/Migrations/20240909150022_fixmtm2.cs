using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagment.Migrations
{
    /// <inheritdoc />
    public partial class fixmtm2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_BillsOfSale_BillOfSaleId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_ShipmentOrders_ShipmentOrderId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_BillOfSaleId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ShipmentOrderId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BillOfSaleId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ShipmentOrderId",
                table: "Items");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillOfSaleItem");

            migrationBuilder.DropTable(
                name: "ItemShipmentOrder");

            migrationBuilder.AddColumn<int>(
                name: "BillOfSaleId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShipmentOrderId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_BillOfSaleId",
                table: "Items",
                column: "BillOfSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ShipmentOrderId",
                table: "Items",
                column: "ShipmentOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_BillsOfSale_BillOfSaleId",
                table: "Items",
                column: "BillOfSaleId",
                principalTable: "BillsOfSale",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ShipmentOrders_ShipmentOrderId",
                table: "Items",
                column: "ShipmentOrderId",
                principalTable: "ShipmentOrders",
                principalColumn: "Id");
        }
    }
}
