// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles account recovery using a recovery code and new password.
/// </summary>
[AllowAnonymous]
internal sealed class RecoverAccountModel(
    IUserManagementService userManagementService,
    IStringLocalizer<AuthResource> localizer,
    IStringLocalizer<UsersResource> usersLocalizer) : PageModel
{
    /// <summary>
    /// Gets or sets the recovery form input.
    /// </summary>
    [BindProperty]
    public RecoverAccountInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the recovery succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Renders the recover account page.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Processes the account recovery request.
    /// </summary>
    /// <returns>The next navigation result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Code))
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.Code)}",
                localizer["Auth.RecoverAccount.Validation.CodeRequired"]);
        }

        if (string.IsNullOrWhiteSpace(Input.NewPassword))
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.NewPassword)}",
                localizer["Auth.RecoverAccount.Validation.PasswordRequired"]);
        }
        else if (Input.NewPassword.Trim().Length < ValidationConstants.PasswordMinLength)
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.NewPassword)}",
                localizer["Auth.RecoverAccount.Validation.PasswordTooShort"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        RecoverAccountRequest request = new(Input.Code!, Input.NewPassword!);

        Result result = await userManagementService
            .RecoverAccountAsync(request, CancellationToken.None)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, ResolveErrorMessage(result.Error!.Code));
            return Page();
        }

        IsSuccess = true;
        return Page();
    }

    private string ResolveErrorMessage(string errorCode)
    {
        LocalizedString usersEntry = usersLocalizer[errorCode];
        if (!usersEntry.ResourceNotFound)
        {
            return usersEntry.Value;
        }

        LocalizedString authEntry = localizer[errorCode];
        if (!authEntry.ResourceNotFound)
        {
            return authEntry.Value;
        }

        return localizer["Auth.RecoverAccount.Error.RecoveryFailed"];
    }

    /// <summary>
    /// Represents the account recovery form input.
    /// </summary>
    internal sealed class RecoverAccountInputModel
    {
        /// <summary>
        /// Gets or sets the recovery code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }
}
