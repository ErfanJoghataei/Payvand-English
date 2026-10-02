using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payvand.DAL.Migrations
{
    /// <inheritdoc />
    public partial class sinkuserandguest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkCount",
                table: "Users");
        }
    }
}
