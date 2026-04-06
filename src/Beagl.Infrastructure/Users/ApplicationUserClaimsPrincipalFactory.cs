// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Security.Claims;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Beagl.Infrastructure.Users;

/// <summary>
/// Extends the default claims principal factory to include the MustChangePassword claim.
/// </summary>
/// <param name="userManager">The identity user manager.</param>
/// <param name="roleManager">The identity role manager.</param>
/// <param name="options">The identity options.</param>
public sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(userManager, roleManager, options)
{
    /// <summary>
    /// The claim type used to indicate that the user must change their password.
    /// </summary>
    public const string MustChangePasswordClaimType = "must_change_password";

    /// <inheritdoc />
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        ClaimsIdentity identity = await base.GenerateClaimsAsync(user).ConfigureAwait(false);

        if (user.MustChangePassword)
        {
            identity.AddClaim(new Claim(MustChangePasswordClaimType, "true"));
        }

        return identity;
    }
}
