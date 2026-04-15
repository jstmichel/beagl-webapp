// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Breeds;

namespace Beagl.Application.Breeds.Dtos;

/// <summary>
/// Represents a breed returned to the UI layer.
/// </summary>
/// <param name="Id">The unique breed identifier.</param>
/// <param name="AnimalType">The type of animal this breed belongs to.</param>
/// <param name="NameEn">The breed name in English.</param>
/// <param name="NameFr">The breed name in French.</param>
/// <param name="IsActive">A value indicating whether the breed is active.</param>
public sealed record BreedDto(
    Guid Id,
    AnimalType AnimalType,
    string NameEn,
    string NameFr,
    bool IsActive);
