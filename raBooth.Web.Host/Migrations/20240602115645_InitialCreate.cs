using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace raBooth.Web.Host.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Collages",
                columns: table => new
                {
                    CollageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CaptureDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CollagePhoto_PhotoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CollagePhoto_Index = table.Column<int>(type: "int", nullable: false),
                    CollagePhoto_CaptureDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CollagePhoto_AddedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CollagePhoto_Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CollagePhoto_DeletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collages", x => x.CollageId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollageSourcePhoto",
                columns: table => new
                {
                    CollageSourcePhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PhotoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CollageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Index = table.Column<int>(type: "int", nullable: false),
                    CaptureDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollageSourcePhoto", x => x.CollageSourcePhotoId);
                    table.ForeignKey(
                        name: "FK_CollageSourcePhoto_Collages_CollageId",
                        column: x => x.CollageId,
                        principalTable: "Collages",
                        principalColumn: "CollageId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CollageSourcePhoto_CollageId",
                table: "CollageSourcePhoto",
                column: "CollageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollageSourcePhoto");

            migrationBuilder.DropTable(
                name: "Collages");
        }
    }
}
