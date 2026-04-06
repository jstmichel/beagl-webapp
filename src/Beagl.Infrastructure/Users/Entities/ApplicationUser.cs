// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Infrastructure.Users.Entities;

/// <summary>
/// Represents an application user, extending ASP.NET Core IdentityUser.
/// </summary>
public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
{
    /// <summary>
    /// Gets or sets the active account recovery code.
    /// </summary>
    public string? RecoveryCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user must change their password at next login.
    /// </summary>
    public bool MustChangePassword { get; set; }
}
