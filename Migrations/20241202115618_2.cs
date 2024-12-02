using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory_Management_System__Miracle_Shop_.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folder_AspNetUsers_UserID",
                table: "Folder");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Folder_FolderID",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_AspNetUsers_UserID",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Folder",
                table: "Folder");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Folder",
                newName: "Folders");

            migrationBuilder.RenameIndex(
                name: "IX_Product_UserID",
                table: "Products",
                newName: "IX_Products_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Product_FolderID",
                table: "Products",
                newName: "IX_Products_FolderID");

            migrationBuilder.RenameIndex(
                name: "IX_Folder_UserID",
                table: "Folders",
                newName: "IX_Folders_UserID");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Folders",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "ProductID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Folders",
                table: "Folders",
                column: "FolderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_AspNetUsers_UserID",
                table: "Folders",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Folders_FolderID",
                table: "Products",
                column: "FolderID",
                principalTable: "Folders",
                principalColumn: "FolderID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_UserID",
                table: "Products",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_AspNetUsers_UserID",
                table: "Folders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Folders_FolderID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_UserID",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Folders",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Folders");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameTable(
                name: "Folders",
                newName: "Folder");

            migrationBuilder.RenameIndex(
                name: "IX_Products_UserID",
                table: "Product",
                newName: "IX_Product_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_FolderID",
                table: "Product",
                newName: "IX_Product_FolderID");

            migrationBuilder.RenameIndex(
                name: "IX_Folders_UserID",
                table: "Folder",
                newName: "IX_Folder_UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "ProductID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Folder",
                table: "Folder",
                column: "FolderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Folder_AspNetUsers_UserID",
                table: "Folder",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Folder_FolderID",
                table: "Product",
                column: "FolderID",
                principalTable: "Folder",
                principalColumn: "FolderID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_AspNetUsers_UserID",
                table: "Product",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
