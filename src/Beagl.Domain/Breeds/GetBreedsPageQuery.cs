// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Breeds;

/// <summary>
/// Represents the parameters for a paginated breed list query.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
/// <param name="AnimalTypeFilter">An optional animal type filter.</param>
/// <param name="IsActiveFilter">An optional active status filter.</param>
public sealed record GetBreedsPageQuery(
    int Page,
    int PageSize,
    AnimalType? AnimalTypeFilter,
    bool? IsActiveFilter);
