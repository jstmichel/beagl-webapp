// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Services;
using Beagl.Domain.Results;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles email confirmation using a token-based flow.
/// </summary>
[AllowAnonymous]
internal sealed class ConfirmEmailModel(
    IUserManagementService userManagementService,
    IStringLocalizer<AuthResource> localizer) : PageModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the confirmation succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an error occurred.
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// Gets or sets the status message to display.
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Processes the email confirmation request.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGetAsync(string? userId, string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            IsError = true;
            StatusMessage = localizer["Auth.ConfirmEmail.Error.MissingToken"];
            return Page();
        }

        Result result = await userManagementService
            .ConfirmAccountByTokenAsync(userId, token, CancellationToken.None)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            IsError = true;
            StatusMessage = localizer["Auth.ConfirmEmail.Error.Failed"];
            return Page();
        }

        IsSuccess = true;
        StatusMessage = localizer["Auth.ConfirmEmail.Success"];
        return Page();
    }
}
