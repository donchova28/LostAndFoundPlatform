using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostAndFoundPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Locations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ApplicationUserId",
                table: "Locations",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_AspNetUsers_ApplicationUserId",
                table: "Locations",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_AspNetUsers_ApplicationUserId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_ApplicationUserId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Locations");
        }
    }
}
