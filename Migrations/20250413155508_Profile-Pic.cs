using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZUVO_MVC_.Migrations
{
    /// <inheritdoc />
    public partial class ProfilePic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicPath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicPath",
                table: "AspNetUsers");
        }
    }
}
