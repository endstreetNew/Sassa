using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class LoDocumentTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoDocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    LoDocumentTypeId = table.Column<int>(nullable: false),
                    DocumentTypeId = table.Column<int>(nullable: false),
                    DisplayName = table.Column<string>(maxLength: 30, nullable: true),
                    DocumentType = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoDocumentTypes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoDocumentTypes");
        }
    }
}
