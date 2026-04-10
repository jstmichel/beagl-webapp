// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
using Beagl.Domain.Breeds;
using Beagl.Infrastructure.Breeds.Entities;

namespace Beagl.Infrastructure.Breeds;

/// <summary>
/// Provides deterministic seed data for known cat and dog breeds.
/// </summary>
[ExcludeFromCodeCoverage]
public static class BreedSeedData
{
    /// <summary>
    /// Returns all seed breed records with fixed GUIDs for EF Core change tracking.
    /// </summary>
    /// <returns>An array of <see cref="BreedEntity"/> seed records.</returns>
    public static BreedEntity[] GetAll() =>
    [
        // ── Dogs ──────────────────────────────────────────────────────────────────
        new BreedEntity
        {
            Id = new Guid("00000001-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Labrador Retriever",
            NameFr = "Labrador",
            DescriptionEn = "Friendly, outgoing, and high-spirited companion.",
            DescriptionFr = "Compagnon amical, sociable et plein d'entrain.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000002-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "German Shepherd",
            NameFr = "Berger allemand",
            DescriptionEn = "Intelligent, loyal, and versatile working dog.",
            DescriptionFr = "Chien de travail intelligent, loyal et polyvalent.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000003-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Golden Retriever",
            NameFr = "Golden Retriever",
            DescriptionEn = "Gentle, reliable family dog with a love for water.",
            DescriptionFr = "Chien de famille doux et fiable qui adore l'eau.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000004-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "French Bulldog",
            NameFr = "Bouledogue français",
            DescriptionEn = "Adaptable, playful, and alert small companion.",
            DescriptionFr = "Petit compagnon adaptable, joueur et alerte.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000005-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Bulldog",
            NameFr = "Bouledogue anglais",
            DescriptionEn = "Calm, courageous, and friendly breed.",
            DescriptionFr = "Race calme, courageuse et amicale.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000006-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Poodle",
            NameFr = "Caniche",
            DescriptionEn = "Highly intelligent and elegant breed in multiple sizes.",
            DescriptionFr = "Race très intelligente et élégante disponible en plusieurs tailles.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000007-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Beagle",
            NameFr = "Beagle",
            DescriptionEn = "Merry, curious hound with an excellent nose.",
            DescriptionFr = "Chien courant joyeux et curieux au flair excellent.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000008-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Rottweiler",
            NameFr = "Rottweiler",
            DescriptionEn = "Confident, powerful guardian and loyal companion.",
            DescriptionFr = "Gardien confiant et puissant, compagnon loyal.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000009-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Dachshund",
            NameFr = "Teckel",
            DescriptionEn = "Bold, lively little dog with a big personality.",
            DescriptionFr = "Petit chien audacieux et vif à forte personnalité.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000a-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Yorkshire Terrier",
            NameFr = "Yorkshire Terrier",
            DescriptionEn = "Feisty, affectionate toy breed with a silky coat.",
            DescriptionFr = "Race miniature fougueuse et affectueuse au pelage soyeux.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000b-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Boxer",
            NameFr = "Boxer",
            DescriptionEn = "Fun-loving, active, and bright family guardian.",
            DescriptionFr = "Gardien familial actif, enjoué et intelligent.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000c-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Siberian Husky",
            NameFr = "Husky sibérien",
            DescriptionEn = "Energetic sled dog bred for endurance.",
            DescriptionFr = "Chien de traîneau énergique élevé pour l'endurance.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000d-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Great Dane",
            NameFr = "Dogue allemand",
            DescriptionEn = "Gentle giant known for its regal appearance.",
            DescriptionFr = "Géant doux connu pour son allure majestueuse.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000e-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Doberman Pinscher",
            NameFr = "Doberman",
            DescriptionEn = "Sleek, powerful, and fearless protector.",
            DescriptionFr = "Protecteur élégant, puissant et intrépide.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000f-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Shih Tzu",
            NameFr = "Shih Tzu",
            DescriptionEn = "Affectionate lap dog with a luxurious coat.",
            DescriptionFr = "Chien de compagnie affectueux au pelage luxueux.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000010-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Border Collie",
            NameFr = "Border Collie",
            DescriptionEn = "Exceptionally smart and agile herding dog.",
            DescriptionFr = "Chien de berger exceptionnellement intelligent et agile.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000011-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Chihuahua",
            NameFr = "Chihuahua",
            DescriptionEn = "Tiny, bold, and charming companion.",
            DescriptionFr = "Compagnon minuscule, audacieux et charmant.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000012-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Pomeranian",
            NameFr = "Poméranien",
            DescriptionEn = "Lively, inquisitive toy breed with a fluffy coat.",
            DescriptionFr = "Race miniature vive et curieuse au pelage duveteux.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000013-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Bernese Mountain Dog",
            NameFr = "Bouvier bernois",
            DescriptionEn = "Good-natured working dog from the Swiss Alps.",
            DescriptionFr = "Chien de travail au bon tempérament originaire des Alpes suisses.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000014-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Australian Shepherd",
            NameFr = "Berger australien",
            DescriptionEn = "Smart, work-oriented herder with striking coat.",
            DescriptionFr = "Berger intelligent et travailleur au pelage remarquable.",
            IsActive = true,
        },

        // ── Cats ──────────────────────────────────────────────────────────────────
        new BreedEntity
        {
            Id = new Guid("00000015-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Domestic Shorthair",
            NameFr = "Chat à poil court",
            DescriptionEn = "Most common cat worldwide, diverse in appearance.",
            DescriptionFr = "Chat le plus répandu au monde, à l'apparence variée.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000016-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Persian",
            NameFr = "Persan",
            DescriptionEn = "Quiet, gentle cat with a long flowing coat.",
            DescriptionFr = "Chat calme et doux au long pelage fluide.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000017-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Maine Coon",
            NameFr = "Maine Coon",
            DescriptionEn = "Large, sociable cat with tufted ears.",
            DescriptionFr = "Grand chat sociable aux oreilles touffues.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000018-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Siamese",
            NameFr = "Siamois",
            DescriptionEn = "Vocal, social cat with striking blue eyes.",
            DescriptionFr = "Chat vocal et sociable aux yeux bleus saisissants.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000019-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Ragdoll",
            NameFr = "Ragdoll",
            DescriptionEn = "Docile, large cat that goes limp when held.",
            DescriptionFr = "Grand chat docile qui se détend quand on le prend.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001a-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Bengal",
            NameFr = "Bengal",
            DescriptionEn = "Athletic, spotted cat with a wild appearance.",
            DescriptionFr = "Chat athlétique et tacheté à l'allure sauvage.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001b-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "British Shorthair",
            NameFr = "British Shorthair",
            DescriptionEn = "Easy-going, round-faced companion.",
            DescriptionFr = "Compagnon facile à vivre au visage rond.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001c-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Abyssinian",
            NameFr = "Abyssin",
            DescriptionEn = "Active, playful cat with a ticked coat.",
            DescriptionFr = "Chat actif et joueur au pelage tiqueté.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001d-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Scottish Fold",
            NameFr = "Scottish Fold",
            DescriptionEn = "Sweet-tempered cat with distinctive folded ears.",
            DescriptionFr = "Chat au tempérament doux aux oreilles repliées distinctives.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001e-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Sphynx",
            NameFr = "Sphynx",
            DescriptionEn = "Hairless, energetic, and affectionate breed.",
            DescriptionFr = "Race sans poil, énergique et affectueuse.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001f-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Russian Blue",
            NameFr = "Bleu russe",
            DescriptionEn = "Shy, gentle cat with a plush blue-grey coat.",
            DescriptionFr = "Chat timide et doux au pelage peluché bleu-gris.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000020-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Norwegian Forest Cat",
            NameFr = "Chat des forêts norvégiennes",
            DescriptionEn = "Hardy, large cat built for cold climates.",
            DescriptionFr = "Grand chat robuste adapté aux climats froids.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000021-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Birman",
            NameFr = "Sacré de Birmanie",
            DescriptionEn = "Gentle, quiet cat with sapphire eyes.",
            DescriptionFr = "Chat doux et calme aux yeux saphir.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000022-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Devon Rex",
            NameFr = "Devon Rex",
            DescriptionEn = "Mischievous, curly-coated cat with large ears.",
            DescriptionFr = "Chat espiègle au pelage bouclé et aux grandes oreilles.",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000023-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Exotic Shorthair",
            NameFr = "Exotic Shorthair",
            DescriptionEn = "Persian-type cat with a short, easy-care coat.",
            DescriptionFr = "Chat de type persan au pelage court et facile d'entretien.",
            IsActive = true,
        },
    ];
}
