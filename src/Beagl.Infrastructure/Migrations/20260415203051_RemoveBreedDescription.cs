using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beagl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBreedDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Breeds");

            migrationBuilder.DropColumn(
                name: "DescriptionFr",
                table: "Breeds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Breeds",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionFr",
                table: "Breeds",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Friendly, outgoing, and high-spirited companion.", "Compagnon amical, sociable et plein d'entrain." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000002-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Intelligent, loyal, and versatile working dog.", "Chien de travail intelligent, loyal et polyvalent." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Gentle, reliable family dog with a love for water.", "Chien de famille doux et fiable qui adore l'eau." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000004-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Adaptable, playful, and alert small companion.", "Petit compagnon adaptable, joueur et alerte." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000005-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Calm, courageous, and friendly breed.", "Race calme, courageuse et amicale." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000006-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Highly intelligent and elegant breed in multiple sizes.", "Race très intelligente et élégante disponible en plusieurs tailles." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000007-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Merry, curious hound with an excellent nose.", "Chien courant joyeux et curieux au flair excellent." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000008-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Confident, powerful guardian and loyal companion.", "Gardien confiant et puissant, compagnon loyal." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000009-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Bold, lively little dog with a big personality.", "Petit chien audacieux et vif à forte personnalité." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000a-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Feisty, affectionate toy breed with a silky coat.", "Race miniature fougueuse et affectueuse au pelage soyeux." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000b-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Fun-loving, active, and bright family guardian.", "Gardien familial actif, enjoué et intelligent." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000c-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Energetic sled dog bred for endurance.", "Chien de traîneau énergique élevé pour l'endurance." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000d-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Gentle giant known for its regal appearance.", "Géant doux connu pour son allure majestueuse." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000e-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Sleek, powerful, and fearless protector.", "Protecteur élégant, puissant et intrépide." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000f-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Affectionate lap dog with a luxurious coat.", "Chien de compagnie affectueux au pelage luxueux." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000010-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Exceptionally smart and agile herding dog.", "Chien de berger exceptionnellement intelligent et agile." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000011-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Tiny, bold, and charming companion.", "Compagnon minuscule, audacieux et charmant." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000012-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Lively, inquisitive toy breed with a fluffy coat.", "Race miniature vive et curieuse au pelage duveteux." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000013-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Good-natured working dog from the Swiss Alps.", "Chien de travail au bon tempérament originaire des Alpes suisses." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000014-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Smart, work-oriented herder with striking coat.", "Berger intelligent et travailleur au pelage remarquable." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000015-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Most common cat worldwide, diverse in appearance.", "Chat le plus répandu au monde, à l'apparence variée." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000016-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Quiet, gentle cat with a long flowing coat.", "Chat calme et doux au long pelage fluide." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000017-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Large, sociable cat with tufted ears.", "Grand chat sociable aux oreilles touffues." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000018-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Vocal, social cat with striking blue eyes.", "Chat vocal et sociable aux yeux bleus saisissants." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000019-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Docile, large cat that goes limp when held.", "Grand chat docile qui se détend quand on le prend." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001a-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Athletic, spotted cat with a wild appearance.", "Chat athlétique et tacheté à l'allure sauvage." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001b-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Easy-going, round-faced companion.", "Compagnon facile à vivre au visage rond." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001c-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Active, playful cat with a ticked coat.", "Chat actif et joueur au pelage tiqueté." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001d-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Sweet-tempered cat with distinctive folded ears.", "Chat au tempérament doux aux oreilles repliées distinctives." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001e-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Hairless, energetic, and affectionate breed.", "Race sans poil, énergique et affectueuse." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001f-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Shy, gentle cat with a plush blue-grey coat.", "Chat timide et doux au pelage peluché bleu-gris." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000020-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Hardy, large cat built for cold climates.", "Grand chat robuste adapté aux climats froids." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000021-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Gentle, quiet cat with sapphire eyes.", "Chat doux et calme aux yeux saphir." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000022-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Mischievous, curly-coated cat with large ears.", "Chat espiègle au pelage bouclé et aux grandes oreilles." });

            migrationBuilder.UpdateData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000023-0000-0000-0000-000000000000"),
                columns: new[] { "DescriptionEn", "DescriptionFr" },
                values: new object[] { "Persian-type cat with a short, easy-care coat.", "Chat de type persan au pelage court et facile d'entretien." });
        }
    }
}
