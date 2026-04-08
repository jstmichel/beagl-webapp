// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
using Beagl.Domain.Users;
using Beagl.Infrastructure.Database;

namespace Beagl.Infrastructure.Users.Entities;

/// <summary>
/// Represents the citizen profile row, linked to a user identity entry.
/// </summary>
[ExcludeFromCodeCoverage]
public class CitizenProfileEntity
{
    /// <summary>
    /// Gets or sets the unique profile identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated user.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current address.
    /// </summary>
    public AddressEntity? Address { get; set; }

    /// <summary>
    /// Gets or sets the date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the communication preference.
    /// </summary>
    public CommunicationPreference CommunicationPreference { get; set; }

    /// <summary>
    /// Gets or sets the language of communication preference.
    /// </summary>
    public LanguagePreference LanguagePreference { get; set; }

    /// <summary>
    /// Gets or sets the associated user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
