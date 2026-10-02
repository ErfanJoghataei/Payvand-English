using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payvand.DAL.Migrations
{
    /// <inheritdoc />
    public partial class editqrcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QrCodes_ShortenedLinks_ShortenedLinkId",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_QrCodes_ShortenedLinkId",
                table: "QrCodes");

            migrationBuilder.RenameColumn(
                name: "config",
                table: "QrCodes",
                newName: "ForeGroundColor");

            migrationBuilder.AlterColumn<int>(
                name: "ShortenedLinkId",
                table: "QrCodes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BackGroundColor",
                table: "QrCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "QrCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "QrCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "QrCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_ShortenedLinkId",
                table: "QrCodes",
                column: "ShortenedLinkId",
                unique: true,
                filter: "[ShortenedLinkId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_QrCodes_ShortenedLinks_ShortenedLinkId",
                table: "QrCodes",
                column: "ShortenedLinkId",
                principalTable: "ShortenedLinks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QrCodes_ShortenedLinks_ShortenedLinkId",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_QrCodes_ShortenedLinkId",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "BackGroundColor",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "QrCodes");

            migrationBuilder.RenameColumn(
                name: "ForeGroundColor",
                table: "QrCodes",
                newName: "config");

            migrationBuilder.AlterColumn<int>(
                name: "ShortenedLinkId",
                table: "QrCodes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_ShortenedLinkId",
                table: "QrCodes",
                column: "ShortenedLinkId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QrCodes_ShortenedLinks_ShortenedLinkId",
                table: "QrCodes",
                column: "ShortenedLinkId",
                principalTable: "ShortenedLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
