using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payvand.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addviolationreport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViolationReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    GuestSessionId = table.Column<int>(type: "int", nullable: true),
                    ShortenedLinkId = table.Column<int>(type: "int", nullable: true),
                    ReportedUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReporterEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViolationReports_GuestSessions_GuestSessionId",
                        column: x => x.GuestSessionId,
                        principalTable: "GuestSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ViolationReports_ShortenedLinks_ShortenedLinkId",
                        column: x => x.ShortenedLinkId,
                        principalTable: "ShortenedLinks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ViolationReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReports_GuestSessionId",
                table: "ViolationReports",
                column: "GuestSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReports_ShortenedLinkId",
                table: "ViolationReports",
                column: "ShortenedLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReports_UserId",
                table: "ViolationReports",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViolationReports");
        }
    }
}
