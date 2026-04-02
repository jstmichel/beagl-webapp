// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain;
using Beagl.Domain.Users;

namespace Beagl.Application.CitizenProfiles.Dtos;

/// <summary>
/// Represents the citizen profile information returned to the UI.
/// </summary>
/// <param name="Id">The unique profile identifier.</param>
/// <param name="UserId">The identifier of the associated user.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Address">The current address.</param>
/// <param name="DateOfBirth">The date of birth.</param>
/// <param name="CommunicationPreference">The communication preference.</param>
/// <param name="LanguagePreference">The language of communication preference.</param>
/// <param name="IsComplete">A value indicating whether the profile is complete.</param>
/// <param name="Email">The email address from Identity.</param>
/// <param name="PhoneNumber">The phone number from Identity.</param>
/// <param name="EmailConfirmed">A value indicating whether the email address is confirmed.</param>
public sealed record CitizenProfileDto(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    Address? Address,
    DateOnly? DateOfBirth,
    CommunicationPreference CommunicationPreference,
    LanguagePreference LanguagePreference,
    bool IsComplete,
    string? Email = null,
    string? PhoneNumber = null,
    bool EmailConfirmed = false);
