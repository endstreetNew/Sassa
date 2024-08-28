using Microsoft.EntityFrameworkCore.Migrations;

namespace Sassa.eForms.Data.Migrations
{
    public partial class BooleanField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SomeBooleanValue",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SomeBooleanValue",
                table: "Users",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);
        }
    }
}
