// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents a user managed by the application.
/// </summary>
/// <param name="Id">The unique user identifier.</param>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email address.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="EmailConfirmed">A value indicating whether the email address is confirmed.</param>
/// <param name="IsLockedOut">A value indicating whether the user is currently locked out.</param>
/// <param name="Role">The user role.</param>
/// <param name="EmailConfirmationToken">The email confirmation token produced at account creation time.</param>
/// <param name="RecoveryCode">The active account recovery code, if any.</param>
public sealed record UserAccount(
    string Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsLockedOut,
    UserRole Role,
    string? EmailConfirmationToken = null,
    string? RecoveryCode = null);
