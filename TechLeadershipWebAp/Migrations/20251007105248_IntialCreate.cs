using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechLeadershipWebAp.Migrations
{
    /// <inheritdoc />
    public partial class IntialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParticipantId = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    ParticipantName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicalLeadScore = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamLeadScore = table.Column<int>(type: "INTEGER", nullable: false),
                    ArchitectScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MentorScore = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectManagerScore = table.Column<int>(type: "INTEGER", nullable: false),
                    DominantLeadershipType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Feedback = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alternatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    LeadershipType = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alternatives_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Text" },
                values: new object[,]
                {
                    { 1, "When your team faces a complex technical challenge, what's your primary approach?" },
                    { 2, "A junior developer is struggling with a task. How do you respond?" },
                    { 3, "Your team needs to choose a new technology stack. What's your role?" }
                });

            migrationBuilder.InsertData(
                table: "Alternatives",
                columns: new[] { "Id", "LeadershipType", "QuestionId", "Text" },
                values: new object[,]
                {
                    { 1, 0, 1, "Break it down into smaller problems and assign based on expertise" },
                    { 2, 1, 1, "Facilitate a team discussion to brainstorm solutions together" },
                    { 3, 2, 1, "Design an architectural solution that addresses the root cause" },
                    { 4, 3, 1, "Pair up developers to work through the challenge collaboratively" },
                    { 5, 4, 1, "Assess impact on timeline and resources, then adjust plans accordingly" },
                    { 6, 0, 2, "Review their code and provide specific technical suggestions" },
                    { 7, 1, 2, "Check if the task was properly explained and offer support" },
                    { 8, 2, 2, "Consider if the architecture or tools are creating unnecessary complexity" },
                    { 9, 3, 2, "Schedule pairing sessions to help them learn through hands-on experience" },
                    { 10, 4, 2, "Evaluate if the task needs to be reassigned or deadline adjusted" },
                    { 11, 0, 3, "Research and present the technical pros and cons of each option" },
                    { 12, 1, 3, "Ensure everyone's opinion is heard and guide toward consensus" },
                    { 13, 2, 3, "Evaluate how each option fits into the long-term system architecture" },
                    { 14, 3, 3, "Help team members understand the learning curve and growth opportunities" },
                    { 15, 4, 3, "Analyze timeline, cost, and resource implications of each option" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alternatives_QuestionId",
                table: "Alternatives",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alternatives");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
