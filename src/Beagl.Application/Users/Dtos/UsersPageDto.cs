// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

/// <summary>
/// Represents a paginated users response.
/// </summary>
/// <param name="Users">The users for the current page.</param>
/// <param name="TotalCount">The total users count for the current filter.</param>
/// <param name="PageNumber">The current page number.</param>
/// <param name="PageSize">The page size.</param>
public sealed record UsersPageDto(
    IReadOnlyList<UserListItemDto> Users,
    int TotalCount,
    int PageNumber,
    int PageSize);
