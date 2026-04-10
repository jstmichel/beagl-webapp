// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Colors.Dtos;

/// <summary>
/// Represents a paginated list of colors returned to the UI layer.
/// </summary>
/// <param name="Items">The colors on the current page.</param>
/// <param name="TotalCount">The total number of colors matching the query.</param>
public sealed record ColorsPageDto(
    IReadOnlyList<ColorDto> Items,
    int TotalCount);
