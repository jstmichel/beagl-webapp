// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Defines the shared authentication flow for username and email sign-in.
/// </summary>
public interface ISharedLoginService
{
    /// <summary>
    /// Attempts to authenticate a user with the shared login flow.
    /// </summary>
    /// <param name="identifier">The user identifier (username or configured email).</param>
    /// <param name="password">The plain text password.</param>
    /// <param name="rememberMe">A value indicating whether the authentication cookie should persist.</param>
    /// <returns>The authentication result status.</returns>
    public Task<SharedLoginStatus> AuthenticateAsync(string identifier, string password, bool rememberMe);

    /// <summary>
    /// Signs out the current user and clears the authentication cookie.
    /// </summary>
    /// <returns>A task that represents the asynchronous sign-out operation.</returns>
    public Task SignOutAsync();

    /// <summary>
    /// Refreshes the authentication cookie for the specified user, updating claims.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task that represents the asynchronous refresh operation.</returns>
    public Task RefreshSignInAsync(string userId);
}
