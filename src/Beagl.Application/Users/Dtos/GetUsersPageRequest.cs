// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

/// <summary>
/// Represents a request for a paginated users list.
/// </summary>
/// <param name="SearchTerm">The optional search term.</param>
/// <param name="PageNumber">The page number to load.</param>
/// <param name="PageSize">The page size.</param>
public sealed record GetUsersPageRequest(
    string? SearchTerm,
    int PageNumber,
    int PageSize);
