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
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000002-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "German Shepherd",
            NameFr = "Berger allemand",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000003-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Golden Retriever",
            NameFr = "Golden Retriever",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000004-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "French Bulldog",
            NameFr = "Bouledogue français",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000005-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Bulldog",
            NameFr = "Bouledogue anglais",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000006-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Poodle",
            NameFr = "Caniche",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000007-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Beagle",
            NameFr = "Beagle",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000008-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Rottweiler",
            NameFr = "Rottweiler",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000009-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Dachshund",
            NameFr = "Teckel",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000a-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Yorkshire Terrier",
            NameFr = "Yorkshire Terrier",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000b-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Boxer",
            NameFr = "Boxer",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000c-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Siberian Husky",
            NameFr = "Husky sibérien",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000d-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Great Dane",
            NameFr = "Dogue allemand",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000e-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Doberman Pinscher",
            NameFr = "Doberman",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000000f-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Shih Tzu",
            NameFr = "Shih Tzu",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000010-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Border Collie",
            NameFr = "Border Collie",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000011-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Chihuahua",
            NameFr = "Chihuahua",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000012-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Pomeranian",
            NameFr = "Poméranien",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000013-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Bernese Mountain Dog",
            NameFr = "Bouvier bernois",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000014-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Dog,
            NameEn = "Australian Shepherd",
            NameFr = "Berger australien",
            IsActive = true,
        },

        // ── Cats ──────────────────────────────────────────────────────────────────
        new BreedEntity
        {
            Id = new Guid("00000015-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Domestic Shorthair",
            NameFr = "Chat à poil court",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000016-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Persian",
            NameFr = "Persan",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000017-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Maine Coon",
            NameFr = "Maine Coon",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000018-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Siamese",
            NameFr = "Siamois",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000019-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Ragdoll",
            NameFr = "Ragdoll",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001a-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Bengal",
            NameFr = "Bengal",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001b-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "British Shorthair",
            NameFr = "British Shorthair",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001c-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Abyssinian",
            NameFr = "Abyssin",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001d-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Scottish Fold",
            NameFr = "Scottish Fold",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001e-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Sphynx",
            NameFr = "Sphynx",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("0000001f-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Russian Blue",
            NameFr = "Bleu russe",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000020-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Norwegian Forest Cat",
            NameFr = "Chat des forêts norvégiennes",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000021-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Birman",
            NameFr = "Sacré de Birmanie",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000022-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Devon Rex",
            NameFr = "Devon Rex",
            IsActive = true,
        },
        new BreedEntity
        {
            Id = new Guid("00000023-0000-0000-0000-000000000000"),
            AnimalType = AnimalType.Cat,
            NameEn = "Exotic Shorthair",
            NameFr = "Exotic Shorthair",
            IsActive = true,
        },
    ];
}
