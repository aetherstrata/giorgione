using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Giorgione.Database.Migrations
{
    /// <inheritdoc />
    public partial class MigrateBirthdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Users" SET "Birthday" = "Birthday" - INTERVAL '2023 years'
                               WHERE date_part('year',"Birthday") = 2024
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Users" SET "Birthday" = "Birthday" + INTERVAL '2023 years'
                               WHERE date_part('year',"Birthday") = 0001
                """);
        }
    }
}
