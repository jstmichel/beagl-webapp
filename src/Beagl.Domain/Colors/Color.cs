// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Colors;

/// <summary>
/// Represents an animal color in the system.
/// </summary>
/// <param name="Id">The unique color identifier.</param>
/// <param name="NameEn">The color name in English.</param>
/// <param name="NameFr">The color name in French.</param>
public sealed record Color(
    Guid Id,
    string NameEn,
    string NameFr);
