using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refriendtableupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Friends_FriendId",
                table: "Friends",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId",
                table: "Friends",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_AppUser_FriendId",
                table: "Friends",
                column: "FriendId",
                principalTable: "AppUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_AppUser_UserId",
                table: "Friends",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_AppUser_FriendId",
                table: "Friends");

            migrationBuilder.DropForeignKey(
                name: "FK_Friends_AppUser_UserId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_FriendId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_UserId",
                table: "Friends");
        }
    }
}
