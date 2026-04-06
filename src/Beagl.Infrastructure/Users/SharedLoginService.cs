// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Beagl.Infrastructure.Users;

/// <summary>
/// Provides a shared sign-in workflow for all users.
/// </summary>
/// <param name="userManager">The identity user manager.</param>
/// <param name="signInManager">The identity sign-in manager.</param>
public sealed class SharedLoginService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : ISharedLoginService
{
    private static readonly HashSet<string> _allowedRoles =
    [
        nameof(UserRole.Citizen),
        nameof(UserRole.Employee),
        nameof(UserRole.Administrator),
    ];

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

        bool hasAccess = await HasAccessAsync(user).ConfigureAwait(false);
        if (!hasAccess)
        {
            return SharedLoginStatus.InvalidCredentials;
        }

        SignInResult signInResult = await signInManager
            .PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true)
            .ConfigureAwait(false);

        if (signInResult.Succeeded)
        {
            if (user.RecoveryCode is not null)
            {
                user.RecoveryCode = null;
                await userManager.UpdateAsync(user).ConfigureAwait(false);
            }

            if (user.MustChangePassword)
            {
                return SharedLoginStatus.MustChangePassword;
            }

            return SharedLoginStatus.Succeeded;
        }

        if (signInResult.IsLockedOut)
        {
            return SharedLoginStatus.LockedOut;
        }

        return SharedLoginStatus.InvalidCredentials;
    }

    /// <inheritdoc />
    public async Task SignOutAsync()
    {
        await signInManager.SignOutAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RefreshSignInAsync(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is not null)
        {
            await signInManager.RefreshSignInAsync(user).ConfigureAwait(false);
        }
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

    private async Task<bool> HasAccessAsync(ApplicationUser user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return roles.Any(role => _allowedRoles.Contains(role));
    }
}
