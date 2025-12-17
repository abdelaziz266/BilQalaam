using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilQalaam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Families",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Families_UserId",
                table: "Families",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_AspNetUsers_UserId",
                table: "Families",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Families_AspNetUsers_UserId",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Families_UserId",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Families");
        }
    }
}
