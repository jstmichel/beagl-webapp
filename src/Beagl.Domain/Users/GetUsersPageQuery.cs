// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents query parameters for a paginated users list.
/// </summary>
/// <param name="SearchTerm">The optional search term.</param>
/// <param name="PageNumber">The page number to load.</param>
/// <param name="PageSize">The page size.</param>
public sealed record GetUsersPageQuery(
    string? SearchTerm,
    int PageNumber,
    int PageSize);
