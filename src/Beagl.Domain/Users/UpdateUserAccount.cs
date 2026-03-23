// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents the data required to update a user account.
/// </summary>
/// <param name="Id">The unique user identifier.</param>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email address.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="Role">The user role.</param>
public sealed record UpdateUserAccount(
    string Id,
    string UserName,
    string? Email,
    string? PhoneNumber,
    UserRole Role);
