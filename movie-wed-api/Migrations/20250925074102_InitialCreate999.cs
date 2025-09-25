using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_wed_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate999 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "COUNTRY",
                table: "USERS",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "STATE",
                table: "USERS",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "COUNTRY",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "STATE",
                table: "USERS");
        }
    }
}
