using Microsoft.EntityFrameworkCore.Migrations;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class InternalDoc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InternalDocument",
                table: "Documents",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalDocument",
                table: "Documents");
        }
    }
}
