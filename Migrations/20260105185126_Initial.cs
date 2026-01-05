using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "latin1");

            migrationBuilder.CreateTable(
                name: "storage",
                columns: table => new
                {
                    storageid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    storageDescription = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    storageAddr1 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    storageAddr2 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    storageCity = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    storageState = table.Column<string>(type: "char(2)", fixedLength: true, maxLength: 2, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    storageZip = table.Column<int>(type: "int", nullable: true),
                    ts_date = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.storageid);
                })
                .Annotation("MySql:CharSet", "latin1")
                .Annotation("Relational:Collation", "latin1_bin");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    key = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    key_expires = table.Column<DateTime>(type: "datetime", nullable: true),
                    password = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    salt = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    last_on = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "latin1")
                .Annotation("Relational:Collation", "latin1_bin");

            migrationBuilder.CreateTable(
                name: "wine",
                columns: table => new
                {
                    wineid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    vineyard = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    label = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    varietal = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1"),
                    vintage = table.Column<int>(type: "int", nullable: true),
                    ts_date = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "latin1_bin")
                        .Annotation("MySql:CharSet", "latin1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.wineid);
                })
                .Annotation("MySql:CharSet", "latin1")
                .Annotation("Relational:Collation", "latin1_bin");

            migrationBuilder.CreateTable(
                name: "bottle",
                columns: table => new
                {
                    bottleid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    wineid = table.Column<int>(type: "int", nullable: false),
                    storageid = table.Column<int>(type: "int", nullable: false),
                    consumed = table.Column<sbyte>(type: "tinyint", nullable: false),
                    binX = table.Column<int>(type: "int", nullable: false),
                    binY = table.Column<int>(type: "int", nullable: false),
                    depth = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    ts_date = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    consumed_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.bottleid);
                    table.ForeignKey(
                        name: "fk_bottles_storage",
                        column: x => x.storageid,
                        principalTable: "storage",
                        principalColumn: "storageid");
                    table.ForeignKey(
                        name: "fk_bottles_wine",
                        column: x => x.wineid,
                        principalTable: "wine",
                        principalColumn: "wineid");
                })
                .Annotation("MySql:CharSet", "latin1")
                .Annotation("Relational:Collation", "latin1_bin");

            migrationBuilder.CreateIndex(
                name: "ix_bottle_storageid",
                table: "bottle",
                column: "storageid");

            migrationBuilder.CreateIndex(
                name: "ix_bottle_wineid",
                table: "bottle",
                column: "wineid");

            migrationBuilder.CreateIndex(
                name: "ix_users_key",
                table: "users",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bottle");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "storage");

            migrationBuilder.DropTable(
                name: "wine");
        }
    }
}
