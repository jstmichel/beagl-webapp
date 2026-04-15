// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Breeds;

/// <summary>
/// Represents an animal breed in the system.
/// </summary>
/// <param name="Id">The unique breed identifier.</param>
/// <param name="AnimalType">The type of animal this breed belongs to.</param>
/// <param name="NameEn">The breed name in English.</param>
/// <param name="NameFr">The breed name in French.</param>
/// <param name="IsActive">A value indicating whether the breed is active.</param>
public sealed record Breed(
    Guid Id,
    AnimalType AnimalType,
    string NameEn,
    string NameFr,
    bool IsActive);
