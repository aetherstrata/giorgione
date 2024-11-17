using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Giorgione.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimeWorldEpisodeCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "anime_feed_channel_id",
                table: "guilds",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "seen_episodes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seen_episodes", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seen_episodes");

            migrationBuilder.DropColumn(
                name: "anime_feed_channel_id",
                table: "guilds");
        }
    }
}
