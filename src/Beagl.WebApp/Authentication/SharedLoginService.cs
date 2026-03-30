// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users.Entities;
using Beagl.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Beagl.WebApp.Authentication;

/// <summary>
/// Provides a shared sign-in workflow for all users.
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

        bool hasAccess = await HasAccessAsync(user).ConfigureAwait(false);
        if (!hasAccess)
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

    private static readonly HashSet<string> _allowedRoles =
    [
        nameof(UserRole.Citizen),
        nameof(UserRole.Employee),
        nameof(UserRole.Administrator),
    ];

    private async Task<bool> HasAccessAsync(ApplicationUser user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return roles.Any(role => _allowedRoles.Contains(role));
    }
}
