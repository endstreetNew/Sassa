using Microsoft.EntityFrameworkCore.Migrations;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class SeperateImabeTaleg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocImage",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "DocImageId",
                table: "Documents",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocImageId",
                table: "Documents");

            migrationBuilder.AddColumn<byte[]>(
                name: "DocImage",
                table: "Documents",
                type: "BLOB",
                maxLength: 400000,
                nullable: true);
        }
    }
}
