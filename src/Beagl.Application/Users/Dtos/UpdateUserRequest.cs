// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

/// <summary>
/// Represents the application request to update a user.
/// </summary>
/// <param name="Id">The unique user identifier.</param>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email address.</param>
/// <param name="PhoneNumber">The phone number.</param>
public sealed record UpdateUserRequest(
    string Id,
    string UserName,
    string Email,
    string? PhoneNumber);
