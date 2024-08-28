using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;
using System;

namespace Sassa.eDocs.Data.Migrations
{
    public partial class DateTostring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DateStamp",
                table: "Documents",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)",
                oldDefaultValueSql: "SYSDATE");

            migrationBuilder.CreateTable(
                name: "DocImage",
                columns: table => new
                {
                    DocImageId = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Image = table.Column<byte[]>(maxLength: 400000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocImage", x => x.DocImageId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocImage");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateStamp",
                table: "Documents",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValueSql: "SYSDATE",
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
