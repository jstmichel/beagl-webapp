// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Colors.Dtos;

/// <summary>
/// Represents a color returned to the UI layer.
/// </summary>
/// <param name="Id">The unique color identifier.</param>
/// <param name="NameEn">The color name in English.</param>
/// <param name="NameFr">The color name in French.</param>
public sealed record ColorDto(
    Guid Id,
    string NameEn,
    string NameFr);
