using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elsa.QuizAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TimeLimit = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.QuizId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestion",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestion", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_QuizQuestion_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizzes",
                columns: table => new
                {
                    UserQuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizzes", x => x.UserQuizId);
                    table.ForeignKey(
                        name: "FK_UserQuizzes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizQuestion",
                columns: table => new
                {
                    UserQuizQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserQuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAnswered = table.Column<bool>(type: "boolean", nullable: false),
                    SubmittedAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizQuestion", x => x.UserQuizQuestionId);
                    table.ForeignKey(
                        name: "FK_UserQuizQuestion_UserQuizzes_UserQuizId",
                        column: x => x.UserQuizId,
                        principalTable: "UserQuizzes",
                        principalColumn: "UserQuizId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestion_QuizId",
                table: "QuizQuestion",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizQuestion_UserQuizId",
                table: "UserQuizQuestion",
                column: "UserQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizzes_UserId",
                table: "UserQuizzes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizQuestion");

            migrationBuilder.DropTable(
                name: "UserQuizQuestion");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "UserQuizzes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
