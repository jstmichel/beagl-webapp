// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Represents the outcome of a shared login attempt.
/// </summary>
public enum SharedLoginStatus
{
    /// <summary>Login succeeded.</summary>
    Succeeded,

    /// <summary>The credentials are invalid.</summary>
    InvalidCredentials,

    /// <summary>The account is locked out.</summary>
    LockedOut,

    /// <summary>The account is not allowed to sign in.</summary>
    NotAllowed,

    /// <summary>The user must change their password before accessing the application.</summary>
    MustChangePassword,
}
