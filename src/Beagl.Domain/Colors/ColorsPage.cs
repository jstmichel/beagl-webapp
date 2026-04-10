// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Colors;

/// <summary>
/// Represents a paginated list of colors.
/// </summary>
/// <param name="Items">The colors on the current page.</param>
/// <param name="TotalCount">The total number of colors matching the query.</param>
public sealed record ColorsPage(
    IReadOnlyList<Color> Items,
    int TotalCount);
