// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.CitizenProfiles.Dtos;

/// <summary>
/// Represents the application request to update a citizen's identity contact information.
/// </summary>
/// <param name="UserId">The identifier of the associated user.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="Email">The optional email address.</param>
public sealed record UpdateCitizenIdentityRequest(
    string UserId,
    string PhoneNumber,
    string? Email);
