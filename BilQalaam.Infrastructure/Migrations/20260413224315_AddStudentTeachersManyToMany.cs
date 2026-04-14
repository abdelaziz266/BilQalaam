using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilQalaam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentTeachersManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ إنشاء جدول StudentTeachers الجديد أولاً
            migrationBuilder.CreateTable(
                name: "StudentTeachers",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTeachers", x => new { x.StudentId, x.TeacherId });
                    table.ForeignKey(
                        name: "FK_StudentTeachers_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentTeachers_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentTeachers_TeacherId",
                table: "StudentTeachers",
                column: "TeacherId");

            // 2️⃣ نقل البيانات من TeacherId إلى StudentTeachers
            migrationBuilder.Sql(@"
                INSERT INTO StudentTeachers (StudentId, TeacherId, AssignedAt)
                SELECT Id, TeacherId, GETUTCDATE()
                FROM Students
                WHERE TeacherId IS NOT NULL AND IsDeleted = 0
            ");

            // 3️⃣ حذف الـ Foreign Key والـ Index القديم
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Teachers_TeacherId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_TeacherId",
                table: "Students");

            // 4️⃣ حذف العمود القديم TeacherId
            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ إعادة إنشاء العمود TeacherId
            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Students",
                type: "int",
                nullable: true);

            // 2️⃣ نقل البيانات من StudentTeachers إلى TeacherId (أول معلم فقط)
            migrationBuilder.Sql(@"
                UPDATE s
                SET s.TeacherId = st.TeacherId
                FROM Students s
                INNER JOIN (
                    SELECT StudentId, MIN(TeacherId) as TeacherId
                    FROM StudentTeachers
                    GROUP BY StudentId
                ) st ON s.Id = st.StudentId
            ");

            // 3️⃣ حذف جدول StudentTeachers
            migrationBuilder.DropTable(
                name: "StudentTeachers");

            // 4️⃣ جعل TeacherId مطلوباً وإضافة الـ Index والـ Foreign Key
            migrationBuilder.Sql(@"
                UPDATE Students SET TeacherId = 0 WHERE TeacherId IS NULL
            ");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_TeacherId",
                table: "Students",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Teachers_TeacherId",
                table: "Students",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
