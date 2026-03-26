// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents the data required to self-register a Citizen account.
/// </summary>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="UserName">The user name.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="Email">The optional email address.</param>
/// <param name="Password">The initial password.</param>
public sealed record RegisterCitizenAccount(
    string FirstName,
    string LastName,
    string UserName,
    string PhoneNumber,
    string? Email,
    string Password);
