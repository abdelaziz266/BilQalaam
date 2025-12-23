using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilQalaam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLessonTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Lessons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_SupervisorId",
                table: "Lessons",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Supervisors_SupervisorId",
                table: "Lessons",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Supervisors_SupervisorId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_SupervisorId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Lessons");
        }
    }
}
