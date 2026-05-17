using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "abbreviation",
                table: "storage",
                type: "varchar(2)",
                maxLength: 2,
                nullable: true,
                collation: "latin1_bin")
                .Annotation("MySql:CharSet", "latin1");

            migrationBuilder.AddColumn<int>(
                name: "columns",
                table: "storage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "has_bottom_bin",
                table: "storage",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_top_bin",
                table: "storage",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "rows",
                table: "storage",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "abbreviation",
                table: "storage");

            migrationBuilder.DropColumn(
                name: "columns",
                table: "storage");

            migrationBuilder.DropColumn(
                name: "has_bottom_bin",
                table: "storage");

            migrationBuilder.DropColumn(
                name: "has_top_bin",
                table: "storage");

            migrationBuilder.DropColumn(
                name: "rows",
                table: "storage");
        }
    }
}
