using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory_Management_System__Miracle_Shop_.Migrations
{
    public partial class product : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderID",
                table: "Product",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Folder",
                columns: table => new
                {
                    FolderID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FolderName = table.Column<string>(nullable: true),
                    UserID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folder", x => x.FolderID);
                    table.ForeignKey(
                        name: "FK_Folder_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Product_FolderID",
                table: "Product",
                column: "FolderID");

            migrationBuilder.CreateIndex(
                name: "IX_Folder_UserID",
                table: "Folder",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Folder_FolderID",
                table: "Product",
                column: "FolderID",
                principalTable: "Folder",
                principalColumn: "FolderID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Folder_FolderID",
                table: "Product");

            migrationBuilder.DropTable(
                name: "Folder");

            migrationBuilder.DropIndex(
                name: "IX_Product_FolderID",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "FolderID",
                table: "Product");
        }
    }
}
