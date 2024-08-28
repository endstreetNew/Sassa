using Microsoft.EntityFrameworkCore.Migrations;

namespace Sassa.eForms.Data.Migrations
{
    public partial class BooleanConvert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CellNumberConfirmed",
                table: "Users",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AddColumn<int>(
                name: "SomeBooleanValue",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SomeBooleanValue",
                table: "Users");

            migrationBuilder.AlterColumn<bool>(
                name: "CellNumberConfirmed",
                table: "Users",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
