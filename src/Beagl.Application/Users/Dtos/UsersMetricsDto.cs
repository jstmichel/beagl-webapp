// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

/// <summary>
/// Represents global users metrics.
/// </summary>
/// <param name="TotalUsers">The total users count.</param>
/// <param name="PendingConfirmationUsers">The users count with unconfirmed email.</param>
/// <param name="LockedOutUsers">The locked out users count.</param>
public sealed record UsersMetricsDto(
    int TotalUsers,
    int PendingConfirmationUsers,
    int LockedOutUsers);
