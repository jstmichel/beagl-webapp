// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
using Beagl.Infrastructure.Colors.Entities;

namespace Beagl.Infrastructure.Colors;

/// <summary>
/// Provides deterministic seed data for common animal coat colors.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ColorSeedData
{
    /// <summary>
    /// Returns all seed color records with fixed GUIDs for EF Core change tracking.
    /// </summary>
    /// <returns>An array of <see cref="ColorEntity"/> seed records.</returns>
    public static ColorEntity[] GetAll() =>
    [
        new ColorEntity { Id = new Guid("10000001-0000-0000-0000-000000000000"), NameEn = "Black", NameFr = "Noir" },
        new ColorEntity { Id = new Guid("10000002-0000-0000-0000-000000000000"), NameEn = "White", NameFr = "Blanc" },
        new ColorEntity { Id = new Guid("10000003-0000-0000-0000-000000000000"), NameEn = "Brown", NameFr = "Brun" },
        new ColorEntity { Id = new Guid("10000004-0000-0000-0000-000000000000"), NameEn = "Gray", NameFr = "Gris" },
        new ColorEntity { Id = new Guid("10000005-0000-0000-0000-000000000000"), NameEn = "Orange", NameFr = "Orange" },
        new ColorEntity { Id = new Guid("10000006-0000-0000-0000-000000000000"), NameEn = "Red", NameFr = "Roux" },
        new ColorEntity { Id = new Guid("10000007-0000-0000-0000-000000000000"), NameEn = "Cream", NameFr = "Crème" },
        new ColorEntity { Id = new Guid("10000008-0000-0000-0000-000000000000"), NameEn = "Golden", NameFr = "Doré" },
        new ColorEntity { Id = new Guid("10000009-0000-0000-0000-000000000000"), NameEn = "Silver", NameFr = "Argenté" },
        new ColorEntity { Id = new Guid("10000010-0000-0000-0000-000000000000"), NameEn = "Blue", NameFr = "Bleu" },
        new ColorEntity { Id = new Guid("10000011-0000-0000-0000-000000000000"), NameEn = "Chocolate", NameFr = "Chocolat" },
        new ColorEntity { Id = new Guid("10000012-0000-0000-0000-000000000000"), NameEn = "Fawn", NameFr = "Fauve" },
        new ColorEntity { Id = new Guid("10000013-0000-0000-0000-000000000000"), NameEn = "Tan", NameFr = "Beige" },
        new ColorEntity { Id = new Guid("10000014-0000-0000-0000-000000000000"), NameEn = "Merle", NameFr = "Merle" },
        new ColorEntity { Id = new Guid("10000015-0000-0000-0000-000000000000"), NameEn = "Brindle", NameFr = "Bringé" },
        new ColorEntity { Id = new Guid("10000016-0000-0000-0000-000000000000"), NameEn = "Sable", NameFr = "Sable" },
        new ColorEntity { Id = new Guid("10000017-0000-0000-0000-000000000000"), NameEn = "Spotted", NameFr = "Tacheté" },
        new ColorEntity { Id = new Guid("10000018-0000-0000-0000-000000000000"), NameEn = "Tricolor", NameFr = "Tricolore" },
        new ColorEntity { Id = new Guid("10000019-0000-0000-0000-000000000000"), NameEn = "Bicolor", NameFr = "Bicolore" },
        new ColorEntity { Id = new Guid("10000020-0000-0000-0000-000000000000"), NameEn = "Tabby", NameFr = "Tigré" },
        new ColorEntity { Id = new Guid("10000021-0000-0000-0000-000000000000"), NameEn = "Calico", NameFr = "Calico" },
        new ColorEntity { Id = new Guid("10000022-0000-0000-0000-000000000000"), NameEn = "Tortoiseshell", NameFr = "Écaille de tortue" },
        new ColorEntity { Id = new Guid("10000023-0000-0000-0000-000000000000"), NameEn = "Seal Point", NameFr = "Seal Point" },
        new ColorEntity { Id = new Guid("10000024-0000-0000-0000-000000000000"), NameEn = "Lilac", NameFr = "Lilas" },
        new ColorEntity { Id = new Guid("10000025-0000-0000-0000-000000000000"), NameEn = "Cinnamon", NameFr = "Cannelle" },
        new ColorEntity { Id = new Guid("10000026-0000-0000-0000-000000000000"), NameEn = "Apricot", NameFr = "Abricot" },
        new ColorEntity { Id = new Guid("10000027-0000-0000-0000-000000000000"), NameEn = "Champagne", NameFr = "Champagne" },
        new ColorEntity { Id = new Guid("10000028-0000-0000-0000-000000000000"), NameEn = "Platinum", NameFr = "Platine" },
        new ColorEntity { Id = new Guid("10000029-0000-0000-0000-000000000000"), NameEn = "Smoke", NameFr = "Fumé" },
        new ColorEntity { Id = new Guid("10000030-0000-0000-0000-000000000000"), NameEn = "Tuxedo", NameFr = "Tuxedo" },
    ];
}
