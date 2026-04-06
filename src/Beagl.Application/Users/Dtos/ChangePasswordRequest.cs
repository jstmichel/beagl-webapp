// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

/// <summary>
/// Represents the application request to change a user password.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="CurrentPassword">The current password.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ChangePasswordRequest(
    string UserId,
    string CurrentPassword,
    string NewPassword);
