// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles user sign-out.
/// </summary>
internal sealed class LogoutModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    /// <summary>
    /// Processes the sign-out request.
    /// </summary>
    /// <returns>A redirect to the login page.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync().ConfigureAwait(false);
        return LocalRedirect("/account/login");
    }
}
