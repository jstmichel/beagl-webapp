// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain.Results;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Handles public self-registration for Citizen accounts.
/// </summary>
[AllowAnonymous]
internal sealed class RegisterModel(
    IUserManagementService userManagementService,
    IStringLocalizer<AuthResource> localizer,
    IStringLocalizer<UsersResource> usersLocalizer) : PageModel
{
    /// <summary>
    /// Gets or sets the registration form payload.
    /// </summary>
    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    /// <summary>
    /// Renders the registration page.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Processes the citizen self-registration request.
    /// </summary>
    /// <returns>The next navigation result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.FirstName))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.FirstName)}", localizer["Auth.Register.Validation.FirstNameRequired"]);
        }

        if (string.IsNullOrWhiteSpace(Input.LastName))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.LastName)}", localizer["Auth.Register.Validation.LastNameRequired"]);
        }

        if (string.IsNullOrWhiteSpace(Input.UserName))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.UserName)}", localizer["Auth.Register.Validation.UserNameRequired"]);
        }

        if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.PhoneNumber)}", localizer["Auth.Register.Validation.PhoneRequired"]);
        }

        if (!string.IsNullOrWhiteSpace(Input.Email))
        {
            EmailAddressAttribute emailValidator = new();
            if (!emailValidator.IsValid(Input.Email.Trim()))
            {
                ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Email)}", localizer["Auth.Register.Validation.EmailInvalid"]);
            }
        }

        if (string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Password)}", localizer["Auth.Register.Validation.PasswordRequired"]);
        }
        else if (Input.Password.Trim().Length < 8)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Password)}", localizer["Auth.Register.Validation.PasswordTooShort"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegisterCitizenRequest request = new(
            Input.FirstName!,
            Input.LastName!,
            Input.UserName!,
            Input.PhoneNumber!,
            string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email,
            Input.Password!);

        Result<UserDetailsDto> result = await userManagementService
            .RegisterCitizenAsync(request, CancellationToken.None)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, ResolveErrorMessage(result.Error!.Code));
            return Page();
        }

        return LocalRedirect("/account/login");
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

        return localizer["Auth.Register.Error.RegistrationFailed"];
    }

    /// <summary>
    /// Represents the citizen self-registration form input.
    /// </summary>
    internal sealed class RegisterInputModel
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the optional email address.
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
