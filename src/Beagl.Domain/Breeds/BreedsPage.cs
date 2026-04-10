// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Breeds;

/// <summary>
/// Represents a paginated list of breeds.
/// </summary>
/// <param name="Items">The breeds on the current page.</param>
/// <param name="TotalCount">The total number of breeds matching the query.</param>
public sealed record BreedsPage(
    IReadOnlyList<Breed> Items,
    int TotalCount);
