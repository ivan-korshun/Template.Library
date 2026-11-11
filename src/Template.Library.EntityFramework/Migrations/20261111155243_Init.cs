using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Library.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    SerialNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Birthday = table.Column<DateOnly>(type: "date", precision: 0, nullable: true),
                    Famous = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.SerialNumber);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    SerialNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PageCount = table.Column<short>(type: "smallint", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.SerialNumber);
                });

            migrationBuilder.CreateTable(
                name: "BookAuthors",
                columns: table => new
                {
                    BookSn = table.Column<int>(type: "int", nullable: false),
                    AuthorSn = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthors", x => new { x.BookSn, x.AuthorSn });
                    table.ForeignKey(
                        name: "FK_BookAuthors_Authors_AuthorSn",
                        column: x => x.AuthorSn,
                        principalTable: "Authors",
                        principalColumn: "SerialNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Books_BookSn",
                        column: x => x.BookSn,
                        principalTable: "Books",
                        principalColumn: "SerialNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authors_FullName",
                table: "Authors",
                column: "FullName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Id",
                table: "Authors",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_AuthorSn",
                table: "BookAuthors",
                column: "AuthorSn");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Id",
                table: "Books",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title",
                table: "Books",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookAuthors");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
