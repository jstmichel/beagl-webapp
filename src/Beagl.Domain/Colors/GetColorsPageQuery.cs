// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Colors;

/// <summary>
/// Represents the parameters for a paginated color list query.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
public sealed record GetColorsPageQuery(
    int Page,
    int PageSize);
