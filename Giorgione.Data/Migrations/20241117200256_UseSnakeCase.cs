using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Giorgione.Database.Migrations
{
    /// <inheritdoc />
    public partial class UseSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Guilds",
                newName: "guilds");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Birthday",
                table: "users",
                newName: "birthday");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "guilds",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StarboardId",
                table: "guilds",
                newName: "starboard_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_guilds",
                table: "guilds",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "gsers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_guilds",
                table: "guilds");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "guilds",
                newName: "Guilds");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Guilds",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "starboard_id",
                table: "Guilds",
                newName: "StarboardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds",
                column: "Id");
        }
    }
}
