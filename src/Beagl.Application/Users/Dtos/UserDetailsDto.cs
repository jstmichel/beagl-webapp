// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

using Beagl.Domain.Users;

/// <summary>
/// Represents the complete user information shown in management screens.
/// </summary>
/// <param name="Id">The unique user identifier.</param>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email address.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="EmailConfirmed">A value indicating whether the email address is confirmed.</param>
/// <param name="IsLockedOut">A value indicating whether the user is currently locked out.</param>
/// <param name="Role">The user role.</param>
/// <param name="EmailConfirmationToken">The email confirmation token produced at account creation time.</param>
public sealed record UserDetailsDto(
    string Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsLockedOut,
    UserRole Role,
    string? EmailConfirmationToken = null);
