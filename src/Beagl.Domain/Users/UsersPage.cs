// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents a paginated users result set.
/// </summary>
/// <param name="Users">The users for the current page.</param>
/// <param name="TotalCount">The total users count for the current filter.</param>
/// <param name="PageNumber">The current page number.</param>
/// <param name="PageSize">The page size.</param>
public sealed record UsersPage(
    IReadOnlyList<UserAccount> Users,
    int TotalCount,
    int PageNumber,
    int PageSize);
