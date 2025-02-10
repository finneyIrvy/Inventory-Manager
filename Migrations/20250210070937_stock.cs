using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory_Management_System__Miracle_Shop_.Migrations
{
    public partial class stock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovement",
                columns: table => new
                {
                    StockMovementID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(nullable: false),
                    MovementType = table.Column<string>(nullable: true),
                    QuantityChanged = table.Column<int>(nullable: false),
                    SourceLocation = table.Column<string>(nullable: true),
                    DestinationLocation = table.Column<string>(nullable: true),
                    MovementDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovement", x => x.StockMovementID);
                    table.ForeignKey(
                        name: "FK_StockMovement_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_ProductID",
                table: "StockMovement",
                column: "ProductID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovement");
        }
    }
}
