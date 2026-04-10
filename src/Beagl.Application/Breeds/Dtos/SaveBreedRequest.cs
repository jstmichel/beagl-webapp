// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Breeds;

namespace Beagl.Application.Breeds.Dtos;

/// <summary>
/// Represents the data required to create or update a breed.
/// </summary>
/// <param name="AnimalType">The type of animal this breed belongs to.</param>
/// <param name="NameEn">The breed name in English.</param>
/// <param name="NameFr">The breed name in French.</param>
/// <param name="DescriptionEn">The breed description in English.</param>
/// <param name="DescriptionFr">The breed description in French.</param>
public sealed record SaveBreedRequest(
    AnimalType AnimalType,
    string NameEn,
    string NameFr,
    string DescriptionEn,
    string DescriptionFr);
