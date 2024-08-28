using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;
using System;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationDocuments",
                columns: table => new
                {
                    ApplicationDocumentId = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    DocumentTypeId = table.Column<int>(nullable: false),
                    ApplicationTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocuments", x => x.ApplicationDocumentId);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationTypes",
                columns: table => new
                {
                    ApplcationTypeId = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 30, nullable: true),
                    Description = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationTypes", x => x.ApplcationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(maxLength: 20, nullable: true),
                    IdNo = table.Column<string>(maxLength: 13, nullable: true),
                    ChildIdNo = table.Column<string>(maxLength: 13, nullable: true),
                    DocumentTypeId = table.Column<int>(nullable: false),
                    User = table.Column<string>(maxLength: 30, nullable: true),
                    DateStamp = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSDATE"),
                    Status = table.Column<int>(nullable: false),
                    DocImage = table.Column<byte[]>(maxLength: 400000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    DocumentTypeId = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 30, nullable: true),
                    Description = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ChildIdNo",
                table: "Documents",
                column: "ChildIdNo");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IdNo",
                table: "Documents",
                column: "IdNo");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Reference",
                table: "Documents",
                column: "Reference");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationDocuments");

            migrationBuilder.DropTable(
                name: "ApplicationTypes");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentTypes");
        }
    }
}
