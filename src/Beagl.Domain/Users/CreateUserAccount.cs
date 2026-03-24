// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents the data required to create a user account.
/// </summary>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email address.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="Password">The initial password.</param>
/// <param name="Role">The target user role.</param>
public sealed record CreateUserAccount(
    string UserName,
    string? Email,
    string? PhoneNumber,
    string Password,
    UserRole Role);
