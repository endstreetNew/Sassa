using Microsoft.EntityFrameworkCore.Migrations;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class ImageIDRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocImageId",
                table: "Documents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocImageId",
                table: "Documents",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);
        }
    }
}
