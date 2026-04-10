// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Colors.Dtos;

/// <summary>
/// Represents the data required to create or update a color.
/// </summary>
/// <param name="NameEn">The color name in English.</param>
/// <param name="NameFr">The color name in French.</param>
public sealed record SaveColorRequest(
    string NameEn,
    string NameFr);
