// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Defines the supported application user roles.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// No role selected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Citizen role.
    /// </summary>
    Citizen = 1,

    /// <summary>
    /// Employee role.
    /// </summary>
    Employee = 2,

    /// <summary>
    /// Administrator role.
    /// </summary>
    Administrator = 3,
}
