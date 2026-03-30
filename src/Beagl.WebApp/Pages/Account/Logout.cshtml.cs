// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles user sign-out.
/// </summary>
internal sealed class LogoutModel(ISharedLoginService sharedLoginService) : PageModel
{
    /// <summary>
    /// Processes the sign-out request.
    /// </summary>
    /// <returns>A redirect to the login page.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        await sharedLoginService.SignOutAsync().ConfigureAwait(false);
        return LocalRedirect("/account/login");
    }
}
