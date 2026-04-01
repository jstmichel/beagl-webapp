// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents a citizen profile with personal information and preferences.
/// </summary>
/// <param name="Id">The unique profile identifier.</param>
/// <param name="UserId">The identifier of the associated user.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Address">The current address.</param>
/// <param name="DateOfBirth">The date of birth.</param>
/// <param name="CommunicationPreference">The communication preference.</param>
/// <param name="LanguagePreference">The language of communication preference.</param>
public sealed record CitizenProfile(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    Address? Address,
    DateOnly? DateOfBirth,
    CommunicationPreference CommunicationPreference,
    LanguagePreference LanguagePreference);
