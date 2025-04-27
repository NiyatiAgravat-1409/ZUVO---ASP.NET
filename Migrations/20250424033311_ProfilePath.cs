using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZUVO_MVC_.Migrations
{
    /// <inheritdoc />
    public partial class ProfilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "HostUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostUserHostId",
                table: "Cars",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_HostUserHostId",
                table: "Cars",
                column: "HostUserHostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_HostUsers_HostUserHostId",
                table: "Cars",
                column: "HostUserHostId",
                principalTable: "HostUsers",
                principalColumn: "HostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cars_HostUsers_HostUserHostId",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "IX_Cars_HostUserHostId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "HostUsers");

            migrationBuilder.DropColumn(
                name: "HostUserHostId",
                table: "Cars");
        }
    }
}
