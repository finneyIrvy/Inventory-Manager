using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory_Management_System__Miracle_Shop_.Migrations
{
    public partial class fin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "Products",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "Products");
        }
    }
}
