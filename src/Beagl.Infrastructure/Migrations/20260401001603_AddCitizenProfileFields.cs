using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beagl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCitizenProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "CitizenProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommunicationPreference",
                table: "CitizenProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "CitizenProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LanguagePreference",
                table: "CitizenProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "CitizenProfiles");

            migrationBuilder.DropColumn(
                name: "CommunicationPreference",
                table: "CitizenProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "CitizenProfiles");

            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                table: "CitizenProfiles");
        }
    }
}
