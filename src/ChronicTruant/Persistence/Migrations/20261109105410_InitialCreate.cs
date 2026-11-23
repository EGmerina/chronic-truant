using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChronicTruant.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "semesters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_days = table.Column<int>(type: "integer", nullable: false),
                    total_pleasure = table.Column<int>(type: "integer", nullable: false),
                    is_expelled = table.Column<bool>(type: "boolean", nullable: false),
                    is_finished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_semesters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "semester_days",
                columns: table => new
                {
                    semester_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<int>(type: "integer", nullable: false),
                    pleasure = table.Column<int>(type: "integer", nullable: false),
                    total_pleasure = table.Column<int>(type: "integer", nullable: false),
                    expelled = table.Column<bool>(type: "boolean", nullable: false),
                    expelled_subject = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_semester_days", x => new { x.semester_id, x.day });
                    table.ForeignKey(
                        name: "fk_semester_days_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teacher_rules",
                columns: table => new
                {
                    semester_id = table.Column<int>(type: "integer", nullable: false),
                    subject = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    rule_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    subject_a = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    subject_b = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    seed = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_rules", x => new { x.semester_id, x.subject });
                    table.ForeignKey(
                        name: "fk_teacher_rules_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    semester_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<int>(type: "integer", nullable: false),
                    subject = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    attended = table.Column<bool>(type: "boolean", nullable: false),
                    was_asked = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons", x => new { x.semester_id, x.day, x.subject });
                    table.ForeignKey(
                        name: "fk_lessons_semester_days_semester_id_day",
                        columns: x => new { x.semester_id, x.day },
                        principalTable: "semester_days",
                        principalColumns: new[] { "semester_id", "day" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_semesters_is_finished",
                table: "semesters",
                column: "is_finished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "teacher_rules");

            migrationBuilder.DropTable(
                name: "semester_days");

            migrationBuilder.DropTable(
                name: "semesters");
        }
    }
}
