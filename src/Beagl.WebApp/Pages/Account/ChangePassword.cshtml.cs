// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles password change for authenticated users.
/// </summary>
[Authorize]
internal sealed class ChangePasswordModel(
    IUserManagementService userManagementService,
    ISharedLoginService sharedLoginService,
    IStringLocalizer<AuthResource> localizer,
    IStringLocalizer<UsersResource> usersLocalizer) : PageModel
{
    /// <summary>
    /// Gets or sets the change password form input.
    /// </summary>
    [BindProperty]
    public ChangePasswordInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the password change succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Renders the change password page.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Processes the password change request.
    /// </summary>
    /// <returns>The next navigation result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.CurrentPassword)}",
                localizer["Auth.ChangePassword.Validation.CurrentPasswordRequired"]);
        }

        if (string.IsNullOrWhiteSpace(Input.NewPassword))
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.NewPassword)}",
                localizer["Auth.ChangePassword.Validation.NewPasswordRequired"]);
        }
        else if (Input.NewPassword.Trim().Length < ValidationConstants.PasswordMinLength)
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.NewPassword)}",
                localizer["Auth.ChangePassword.Validation.PasswordTooShort"]);
        }

        if (!string.IsNullOrWhiteSpace(Input.NewPassword)
            && !string.Equals(Input.NewPassword, Input.ConfirmNewPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.ConfirmNewPassword)}",
                localizer["Auth.ChangePassword.Validation.PasswordMismatch"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(string.Empty, localizer["Auth.ChangePassword.Error.ChangeFailed"]);
            return Page();
        }

        ChangePasswordRequest request = new(userId, Input.CurrentPassword!, Input.NewPassword!);

        Result result = await userManagementService
            .ChangePasswordAsync(request, CancellationToken.None)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, ResolveErrorMessage(result.Error!.Code));
            return Page();
        }

        await sharedLoginService.RefreshSignInAsync(userId).ConfigureAwait(false);

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

        return localizer["Auth.ChangePassword.Error.ChangeFailed"];
    }

    /// <summary>
    /// Represents the change password form input.
    /// </summary>
    internal sealed class ChangePasswordInputModel
    {
        /// <summary>
        /// Gets or sets the current password.
        /// </summary>
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the confirmed new password.
        /// </summary>
        [DataType(DataType.Password)]
        public string? ConfirmNewPassword { get; set; }
    }
}
