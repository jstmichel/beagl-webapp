// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;

namespace Beagl.Infrastructure.Colors.Entities;

/// <summary>
/// Represents the persisted animal color row.
/// </summary>
[ExcludeFromCodeCoverage]
public class ColorEntity
{
    /// <summary>
    /// Gets or sets the unique color identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the color name in English.
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color name in French.
    /// </summary>
    public string NameFr { get; set; } = string.Empty;
}
