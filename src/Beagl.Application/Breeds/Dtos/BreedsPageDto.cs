// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Breeds.Dtos;

/// <summary>
/// Represents a paginated list of breeds returned to the UI layer.
/// </summary>
/// <param name="Items">The breeds on the current page.</param>
/// <param name="TotalCount">The total number of breeds matching the query.</param>
public sealed record BreedsPageDto(
    IReadOnlyList<BreedDto> Items,
    int TotalCount);
