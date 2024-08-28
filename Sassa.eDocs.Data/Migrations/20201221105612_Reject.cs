using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class Reject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectReasonId",
                table: "Documents",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RejectReasons",
                columns: table => new
                {
                    RejectReasonId = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Reason = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectReasons", x => x.RejectReasonId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RejectReasons");

            migrationBuilder.DropColumn(
                name: "RejectReasonId",
                table: "Documents");
        }
    }
}
