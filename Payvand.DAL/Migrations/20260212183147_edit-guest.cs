using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payvand.DAL.Migrations
{
    /// <inheritdoc />
    public partial class editguest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IpAdress",
                table: "GuestSessions",
                newName: "IpAddress");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "GuestSessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ConvertedAt",
                table: "GuestSessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_GuestSessions_UserId",
                table: "GuestSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestSessions_Users_UserId",
                table: "GuestSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestSessions_Users_UserId",
                table: "GuestSessions");

            migrationBuilder.DropIndex(
                name: "IX_GuestSessions_UserId",
                table: "GuestSessions");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "GuestSessions",
                newName: "IpAdress");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "GuestSessions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ConvertedAt",
                table: "GuestSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
