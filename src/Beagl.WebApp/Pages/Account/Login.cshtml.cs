// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Domain.Users;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles the shared login experience for all users.
/// </summary>
[AllowAnonymous]
internal sealed class LoginModel(
    ISharedLoginService sharedLoginService,
    IStringLocalizer<AuthResource> localizer) : PageModel
{
    /// <summary>
    /// Gets or sets the login form payload.
    /// </summary>
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets or sets the return URL after a successful login.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Renders the login screen.
    /// </summary>
    /// <param name="returnUrl">The optional return URL.</param>
    public void OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl;
    }

    /// <summary>
    /// Processes a shared login request.
    /// </summary>
    /// <param name="returnUrl">The optional return URL.</param>
    /// <returns>The next navigation result.</returns>
    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        ReturnUrl = returnUrl;

        if (string.IsNullOrWhiteSpace(Input.Identifier))
        {
            ModelState.AddModelError(nameof(Input.Identifier), localizer["Auth.Login.Validation.IdentifierRequired"]);
        }

        if (string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.AddModelError(nameof(Input.Password), localizer["Auth.Login.Validation.PasswordRequired"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        SharedLoginStatus status = await sharedLoginService
            .AuthenticateAsync(Input.Identifier, Input.Password, Input.RememberMe)
            .ConfigureAwait(false);

        if (status == SharedLoginStatus.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return LocalRedirect("/");
        }

        string message = status switch
        {
            SharedLoginStatus.LockedOut => localizer["Auth.Login.Error.LockedOut"],
            SharedLoginStatus.NotAllowed => localizer["Auth.Login.Error.NotAllowed"],
            _ => localizer["Auth.Login.Error.InvalidCredentials"],
        };

        ModelState.AddModelError(string.Empty, message);
        return Page();
    }

    /// <summary>
    /// Represents the shared login form input.
    /// </summary>
    internal sealed class LoginInputModel
    {
        /// <summary>
        /// Gets or sets the shared identifier (username or configured email).
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the session should persist.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
