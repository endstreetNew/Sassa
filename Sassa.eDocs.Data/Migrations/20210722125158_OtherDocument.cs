using Microsoft.EntityFrameworkCore.Migrations;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class OtherDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OtherDocumentType",
                table: "Documents",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherDocumentType",
                table: "Documents");
        }
    }
}
