using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beagl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAddressWithStructuredFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "CitizenProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "CitizenProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "CitizenProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Province",
                table: "CitizenProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "CitizenProfiles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "CitizenProfiles");

            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "CitizenProfiles");

            migrationBuilder.DropColumn(
                name: "Address_Province",
                table: "CitizenProfiles");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "CitizenProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "CitizenProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
