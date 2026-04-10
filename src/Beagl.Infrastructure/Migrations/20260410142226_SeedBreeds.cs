using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Beagl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Breeds",
                columns: new[] { "Id", "AnimalType", "DescriptionEn", "DescriptionFr", "IsActive", "NameEn", "NameFr" },
                values: new object[,]
                {
                    { new Guid("00000001-0000-0000-0000-000000000000"), 2, "Friendly, outgoing, and high-spirited companion.", "Compagnon amical, sociable et plein d'entrain.", true, "Labrador Retriever", "Labrador" },
                    { new Guid("00000002-0000-0000-0000-000000000000"), 2, "Intelligent, loyal, and versatile working dog.", "Chien de travail intelligent, loyal et polyvalent.", true, "German Shepherd", "Berger allemand" },
                    { new Guid("00000003-0000-0000-0000-000000000000"), 2, "Gentle, reliable family dog with a love for water.", "Chien de famille doux et fiable qui adore l'eau.", true, "Golden Retriever", "Golden Retriever" },
                    { new Guid("00000004-0000-0000-0000-000000000000"), 2, "Adaptable, playful, and alert small companion.", "Petit compagnon adaptable, joueur et alerte.", true, "French Bulldog", "Bouledogue français" },
                    { new Guid("00000005-0000-0000-0000-000000000000"), 2, "Calm, courageous, and friendly breed.", "Race calme, courageuse et amicale.", true, "Bulldog", "Bouledogue anglais" },
                    { new Guid("00000006-0000-0000-0000-000000000000"), 2, "Highly intelligent and elegant breed in multiple sizes.", "Race très intelligente et élégante disponible en plusieurs tailles.", true, "Poodle", "Caniche" },
                    { new Guid("00000007-0000-0000-0000-000000000000"), 2, "Merry, curious hound with an excellent nose.", "Chien courant joyeux et curieux au flair excellent.", true, "Beagle", "Beagle" },
                    { new Guid("00000008-0000-0000-0000-000000000000"), 2, "Confident, powerful guardian and loyal companion.", "Gardien confiant et puissant, compagnon loyal.", true, "Rottweiler", "Rottweiler" },
                    { new Guid("00000009-0000-0000-0000-000000000000"), 2, "Bold, lively little dog with a big personality.", "Petit chien audacieux et vif à forte personnalité.", true, "Dachshund", "Teckel" },
                    { new Guid("0000000a-0000-0000-0000-000000000000"), 2, "Feisty, affectionate toy breed with a silky coat.", "Race miniature fougueuse et affectueuse au pelage soyeux.", true, "Yorkshire Terrier", "Yorkshire Terrier" },
                    { new Guid("0000000b-0000-0000-0000-000000000000"), 2, "Fun-loving, active, and bright family guardian.", "Gardien familial actif, enjoué et intelligent.", true, "Boxer", "Boxer" },
                    { new Guid("0000000c-0000-0000-0000-000000000000"), 2, "Energetic sled dog bred for endurance.", "Chien de traîneau énergique élevé pour l'endurance.", true, "Siberian Husky", "Husky sibérien" },
                    { new Guid("0000000d-0000-0000-0000-000000000000"), 2, "Gentle giant known for its regal appearance.", "Géant doux connu pour son allure majestueuse.", true, "Great Dane", "Dogue allemand" },
                    { new Guid("0000000e-0000-0000-0000-000000000000"), 2, "Sleek, powerful, and fearless protector.", "Protecteur élégant, puissant et intrépide.", true, "Doberman Pinscher", "Doberman" },
                    { new Guid("0000000f-0000-0000-0000-000000000000"), 2, "Affectionate lap dog with a luxurious coat.", "Chien de compagnie affectueux au pelage luxueux.", true, "Shih Tzu", "Shih Tzu" },
                    { new Guid("00000010-0000-0000-0000-000000000000"), 2, "Exceptionally smart and agile herding dog.", "Chien de berger exceptionnellement intelligent et agile.", true, "Border Collie", "Border Collie" },
                    { new Guid("00000011-0000-0000-0000-000000000000"), 2, "Tiny, bold, and charming companion.", "Compagnon minuscule, audacieux et charmant.", true, "Chihuahua", "Chihuahua" },
                    { new Guid("00000012-0000-0000-0000-000000000000"), 2, "Lively, inquisitive toy breed with a fluffy coat.", "Race miniature vive et curieuse au pelage duveteux.", true, "Pomeranian", "Poméranien" },
                    { new Guid("00000013-0000-0000-0000-000000000000"), 2, "Good-natured working dog from the Swiss Alps.", "Chien de travail au bon tempérament originaire des Alpes suisses.", true, "Bernese Mountain Dog", "Bouvier bernois" },
                    { new Guid("00000014-0000-0000-0000-000000000000"), 2, "Smart, work-oriented herder with striking coat.", "Berger intelligent et travailleur au pelage remarquable.", true, "Australian Shepherd", "Berger australien" },
                    { new Guid("00000015-0000-0000-0000-000000000000"), 1, "Most common cat worldwide, diverse in appearance.", "Chat le plus répandu au monde, à l'apparence variée.", true, "Domestic Shorthair", "Chat à poil court" },
                    { new Guid("00000016-0000-0000-0000-000000000000"), 1, "Quiet, gentle cat with a long flowing coat.", "Chat calme et doux au long pelage fluide.", true, "Persian", "Persan" },
                    { new Guid("00000017-0000-0000-0000-000000000000"), 1, "Large, sociable cat with tufted ears.", "Grand chat sociable aux oreilles touffues.", true, "Maine Coon", "Maine Coon" },
                    { new Guid("00000018-0000-0000-0000-000000000000"), 1, "Vocal, social cat with striking blue eyes.", "Chat vocal et sociable aux yeux bleus saisissants.", true, "Siamese", "Siamois" },
                    { new Guid("00000019-0000-0000-0000-000000000000"), 1, "Docile, large cat that goes limp when held.", "Grand chat docile qui se détend quand on le prend.", true, "Ragdoll", "Ragdoll" },
                    { new Guid("0000001a-0000-0000-0000-000000000000"), 1, "Athletic, spotted cat with a wild appearance.", "Chat athlétique et tacheté à l'allure sauvage.", true, "Bengal", "Bengal" },
                    { new Guid("0000001b-0000-0000-0000-000000000000"), 1, "Easy-going, round-faced companion.", "Compagnon facile à vivre au visage rond.", true, "British Shorthair", "British Shorthair" },
                    { new Guid("0000001c-0000-0000-0000-000000000000"), 1, "Active, playful cat with a ticked coat.", "Chat actif et joueur au pelage tiqueté.", true, "Abyssinian", "Abyssin" },
                    { new Guid("0000001d-0000-0000-0000-000000000000"), 1, "Sweet-tempered cat with distinctive folded ears.", "Chat au tempérament doux aux oreilles repliées distinctives.", true, "Scottish Fold", "Scottish Fold" },
                    { new Guid("0000001e-0000-0000-0000-000000000000"), 1, "Hairless, energetic, and affectionate breed.", "Race sans poil, énergique et affectueuse.", true, "Sphynx", "Sphynx" },
                    { new Guid("0000001f-0000-0000-0000-000000000000"), 1, "Shy, gentle cat with a plush blue-grey coat.", "Chat timide et doux au pelage peluché bleu-gris.", true, "Russian Blue", "Bleu russe" },
                    { new Guid("00000020-0000-0000-0000-000000000000"), 1, "Hardy, large cat built for cold climates.", "Grand chat robuste adapté aux climats froids.", true, "Norwegian Forest Cat", "Chat des forêts norvégiennes" },
                    { new Guid("00000021-0000-0000-0000-000000000000"), 1, "Gentle, quiet cat with sapphire eyes.", "Chat doux et calme aux yeux saphir.", true, "Birman", "Sacré de Birmanie" },
                    { new Guid("00000022-0000-0000-0000-000000000000"), 1, "Mischievous, curly-coated cat with large ears.", "Chat espiègle au pelage bouclé et aux grandes oreilles.", true, "Devon Rex", "Devon Rex" },
                    { new Guid("00000023-0000-0000-0000-000000000000"), 1, "Persian-type cat with a short, easy-care coat.", "Chat de type persan au pelage court et facile d'entretien.", true, "Exotic Shorthair", "Exotic Shorthair" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000002-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000004-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000005-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000006-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000007-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000008-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000009-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000a-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000b-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000c-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000d-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000e-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000000f-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000010-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000011-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000012-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000013-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000014-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000015-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000016-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000017-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000018-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000019-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001a-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001b-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001c-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001d-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001e-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("0000001f-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000020-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000021-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000022-0000-0000-0000-000000000000"));

            migrationBuilder.DeleteData(
                table: "Breeds",
                keyColumn: "Id",
                keyValue: new Guid("00000023-0000-0000-0000-000000000000"));
        }
    }
}
