using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMusicPlayer.Migrations
{
    public partial class AddPlaylistCoverPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverPath",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverPath",
                table: "Playlists");
        }
    }
}