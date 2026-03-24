// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.WebApp.Authentication;

/// <summary>
/// Defines the shared authentication flow for username and email sign-in.
/// </summary>
internal interface ISharedLoginService
{
    /// <summary>
    /// Attempts to authenticate a user with the shared login flow.
    /// </summary>
    /// <param name="identifier">The user identifier (username or configured email).</param>
    /// <param name="password">The plain text password.</param>
    /// <param name="rememberMe">A value indicating whether the authentication cookie should persist.</param>
    /// <returns>The authentication result status.</returns>
    public Task<SharedLoginStatus> AuthenticateAsync(string identifier, string password, bool rememberMe);
}
