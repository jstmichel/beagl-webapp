// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain;

/// <summary>
/// Shared validation limits used across application and UI layers.
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Maximum allowed length for a user name.
    /// </summary>
    public const int UserNameMaxLength = 256;

    /// <summary>
    /// Minimum allowed length for a password.
    /// </summary>
    public const int PasswordMinLength = 8;
}
