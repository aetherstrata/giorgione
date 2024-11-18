using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Giorgione.Database.Migrations
{
    /// <inheritdoc />
    public partial class GuildBirthdayChannelId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "birthday_channel_id",
                table: "guilds",
                type: "numeric(20,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birthday_channel_id",
                table: "guilds");
        }
    }
}
