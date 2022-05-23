using Microsoft.EntityFrameworkCore.Migrations;

namespace HaT7FptBook.Migrations
{
    public partial class UpdateBookModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Books_StoreId",
                table: "Books",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Stores_StoreId",
                table: "Books",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Stores_StoreId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_StoreId",
                table: "Books");
        }
    }
}
