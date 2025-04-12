using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserCrud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUser",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "NVARCHAR(450)", maxLength: 450, nullable: false),
                    UserName = table.Column<string>(type: "NVARCHAR(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR(200)", maxLength: 200, nullable: false),
                    FirstName = table.Column<string>(type: "NVARCHAR(25)", maxLength: 25, nullable: false),
                    LastName = table.Column<string>(type: "NVARCHAR(25)", maxLength: 25, nullable: false),
                    Gender = table.Column<string>(type: "NVARCHAR(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CountryName = table.Column<string>(type: "NVARCHAR(250)", maxLength: 250, nullable: false),
                    StateName = table.Column<string>(type: "NVARCHAR(250)", maxLength: 250, nullable: false),
                    CityName = table.Column<string>(type: "NVARCHAR(250)", maxLength: 250, nullable: false),
                    Address = table.Column<string>(type: "NVARCHAR(1000)", maxLength: 1000, nullable: false),
                    BlockedReason = table.Column<string>(type: "NVARCHAR(1500)", maxLength: 1500, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "NVARCHAR(300)", maxLength: 300, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUser");
        }
    }
}
