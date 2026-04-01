// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain;
using Beagl.Domain.Users;

namespace Beagl.Application.CitizenProfiles.Dtos;

/// <summary>
/// Represents the application request to update a citizen profile.
/// </summary>
/// <param name="UserId">The identifier of the associated user.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Address">The current address.</param>
/// <param name="DateOfBirth">The date of birth.</param>
/// <param name="CommunicationPreference">The communication preference.</param>
/// <param name="LanguagePreference">The language of communication preference.</param>
public sealed record UpdateCitizenProfileRequest(
    string UserId,
    string FirstName,
    string LastName,
    Address Address,
    DateOnly DateOfBirth,
    CommunicationPreference CommunicationPreference,
    LanguagePreference LanguagePreference);
