using Microsoft.EntityFrameworkCore.Migrations;

namespace HaT7FptBook.Migrations
{
    public partial class UpdateProductISBN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ISBN",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISBN",
                table: "Products");
        }
    }
}
