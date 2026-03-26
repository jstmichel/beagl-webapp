// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Infrastructure.Users.Entities;

/// <summary>
/// Represents the citizen profile row, linked to a user identity entry.
/// </summary>
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
    /// Gets or sets the associated user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
