// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

/// <summary>
/// Represents the application request to create a user.
/// </summary>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email address.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="Password">The initial password.</param>
public sealed record CreateUserRequest(
    string UserName,
    string Email,
    string? PhoneNumber,
    string Password);
