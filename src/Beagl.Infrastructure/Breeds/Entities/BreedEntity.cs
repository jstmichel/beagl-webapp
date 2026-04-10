// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
using Beagl.Domain.Breeds;

namespace Beagl.Infrastructure.Breeds.Entities;

/// <summary>
/// Represents the persisted animal breed row.
/// </summary>
[ExcludeFromCodeCoverage]
public class BreedEntity
{
    /// <summary>
    /// Gets or sets the unique breed identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type of animal this breed belongs to.
    /// </summary>
    public AnimalType AnimalType { get; set; }

    /// <summary>
    /// Gets or sets the breed name in English.
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the breed name in French.
    /// </summary>
    public string NameFr { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the breed description in English.
    /// </summary>
    public string DescriptionEn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the breed description in French.
    /// </summary>
    public string DescriptionFr { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the breed is active.
    /// </summary>
    public bool IsActive { get; set; }
}
