using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Beagl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameFr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Colors",
                columns: new[] { "Id", "NameEn", "NameFr" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000000"), "Black", "Noir" },
                    { new Guid("10000002-0000-0000-0000-000000000000"), "White", "Blanc" },
                    { new Guid("10000003-0000-0000-0000-000000000000"), "Brown", "Brun" },
                    { new Guid("10000004-0000-0000-0000-000000000000"), "Gray", "Gris" },
                    { new Guid("10000005-0000-0000-0000-000000000000"), "Orange", "Orange" },
                    { new Guid("10000006-0000-0000-0000-000000000000"), "Red", "Roux" },
                    { new Guid("10000007-0000-0000-0000-000000000000"), "Cream", "Crème" },
                    { new Guid("10000008-0000-0000-0000-000000000000"), "Golden", "Doré" },
                    { new Guid("10000009-0000-0000-0000-000000000000"), "Silver", "Argenté" },
                    { new Guid("10000010-0000-0000-0000-000000000000"), "Blue", "Bleu" },
                    { new Guid("10000011-0000-0000-0000-000000000000"), "Chocolate", "Chocolat" },
                    { new Guid("10000012-0000-0000-0000-000000000000"), "Fawn", "Fauve" },
                    { new Guid("10000013-0000-0000-0000-000000000000"), "Tan", "Beige" },
                    { new Guid("10000014-0000-0000-0000-000000000000"), "Merle", "Merle" },
                    { new Guid("10000015-0000-0000-0000-000000000000"), "Brindle", "Bringé" },
                    { new Guid("10000016-0000-0000-0000-000000000000"), "Sable", "Sable" },
                    { new Guid("10000017-0000-0000-0000-000000000000"), "Spotted", "Tacheté" },
                    { new Guid("10000018-0000-0000-0000-000000000000"), "Tricolor", "Tricolore" },
                    { new Guid("10000019-0000-0000-0000-000000000000"), "Bicolor", "Bicolore" },
                    { new Guid("10000020-0000-0000-0000-000000000000"), "Tabby", "Tigré" },
                    { new Guid("10000021-0000-0000-0000-000000000000"), "Calico", "Calico" },
                    { new Guid("10000022-0000-0000-0000-000000000000"), "Tortoiseshell", "Écaille de tortue" },
                    { new Guid("10000023-0000-0000-0000-000000000000"), "Seal Point", "Seal Point" },
                    { new Guid("10000024-0000-0000-0000-000000000000"), "Lilac", "Lilas" },
                    { new Guid("10000025-0000-0000-0000-000000000000"), "Cinnamon", "Cannelle" },
                    { new Guid("10000026-0000-0000-0000-000000000000"), "Apricot", "Abricot" },
                    { new Guid("10000027-0000-0000-0000-000000000000"), "Champagne", "Champagne" },
                    { new Guid("10000028-0000-0000-0000-000000000000"), "Platinum", "Platine" },
                    { new Guid("10000029-0000-0000-0000-000000000000"), "Smoke", "Fumé" },
                    { new Guid("10000030-0000-0000-0000-000000000000"), "Tuxedo", "Tuxedo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colors");
        }
    }
}
