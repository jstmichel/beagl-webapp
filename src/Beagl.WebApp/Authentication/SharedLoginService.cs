// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Beagl.WebApp.Authentication;

/// <summary>
/// Provides a shared sign-in workflow for employees and citizens.
/// </summary>
internal sealed class SharedLoginService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : ISharedLoginService
{
    /// <inheritdoc />
    public async Task<SharedLoginStatus> AuthenticateAsync(string identifier, string password, bool rememberMe)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(password);

        string normalizedIdentifier = identifier.Trim();
        ApplicationUser? user = await FindUserByIdentifierAsync(normalizedIdentifier).ConfigureAwait(false);
        if (user is null)
        {
            return SharedLoginStatus.InvalidCredentials;
        }

        SignInResult signInResult = await signInManager
            .PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false)
            .ConfigureAwait(false);

        if (signInResult.Succeeded)
        {
            return SharedLoginStatus.Succeeded;
        }

        if (signInResult.IsLockedOut)
        {
            return SharedLoginStatus.LockedOut;
        }

        if (signInResult.IsNotAllowed)
        {
            return SharedLoginStatus.NotAllowed;
        }

        return SharedLoginStatus.InvalidCredentials;
    }

    private async Task<ApplicationUser?> FindUserByIdentifierAsync(string identifier)
    {
        ApplicationUser? userNameUser = await userManager.FindByNameAsync(identifier).ConfigureAwait(false);
        if (userNameUser is not null)
        {
            return userNameUser;
        }

        ApplicationUser? emailUser = await userManager.FindByEmailAsync(identifier).ConfigureAwait(false);
        return emailUser;
    }
}
