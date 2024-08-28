using Microsoft.EntityFrameworkCore.Migrations;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class RejectResonstring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectReasonId",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "Documents",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "RejectReasonId",
                table: "Documents",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
