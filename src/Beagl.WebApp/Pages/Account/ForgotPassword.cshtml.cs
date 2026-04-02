// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Services;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles the forgot password request to generate a recovery code.
/// </summary>
[AllowAnonymous]
internal sealed class ForgotPasswordModel(
    IUserManagementService userManagementService,
    IStringLocalizer<AuthResource> localizer) : PageModel
{
    /// <summary>
    /// Gets or sets the form input.
    /// </summary>
    [BindProperty]
    public ForgotPasswordInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the form was submitted successfully.
    /// </summary>
    public bool IsSubmitted { get; set; }

    /// <summary>
    /// Renders the forgot password page.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Processes the forgot password request.
    /// </summary>
    /// <returns>The next navigation result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Identifier))
        {
            ModelState.AddModelError(
                nameof(Input.Identifier),
                localizer["Auth.ForgotPassword.Validation.IdentifierRequired"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await userManagementService
            .RequestRecoveryCodeAsync(Input.Identifier!, CancellationToken.None)
            .ConfigureAwait(false);

        IsSubmitted = true;
        return Page();
    }

    /// <summary>
    /// Represents the forgot password form input.
    /// </summary>
    internal sealed class ForgotPasswordInputModel
    {
        /// <summary>
        /// Gets or sets the identifier (username or email).
        /// </summary>
        public string? Identifier { get; set; }
    }
}
