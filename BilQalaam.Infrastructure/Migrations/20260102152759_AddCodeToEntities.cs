using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilQalaam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Families",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Families");
        }
    }
}
