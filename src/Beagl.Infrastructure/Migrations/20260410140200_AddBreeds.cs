using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beagl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Breeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalType = table.Column<int>(type: "integer", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameFr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DescriptionFr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeds", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Breeds");
        }
    }
}
