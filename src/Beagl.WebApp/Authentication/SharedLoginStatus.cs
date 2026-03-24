// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.WebApp.Authentication;

/// <summary>
/// Represents the outcome of a shared login attempt.
/// </summary>
internal enum SharedLoginStatus
{
    Succeeded,
    InvalidCredentials,
    LockedOut,
    NotAllowed,
}
