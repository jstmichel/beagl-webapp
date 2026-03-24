// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents global users metrics.
/// </summary>
/// <param name="TotalUsers">The total users count.</param>
/// <param name="PendingConfirmationUsers">The users count with unconfirmed email.</param>
/// <param name="LockedOutUsers">The locked out users count.</param>
public sealed record UsersMetrics(
    int TotalUsers,
    int PendingConfirmationUsers,
    int LockedOutUsers);
