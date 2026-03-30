// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Application.EmailProviders.Dtos;
using Beagl.Application.EmailProviders.Services;
using Beagl.Application.Setup.Dtos;
using Beagl.Application.Setup.Services;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.WebApp.Extensions;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Pages;

/// <summary>
/// Code-behind for the initial setup wizard page.
/// </summary>
public sealed partial class Setup : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly SetupFormModel _form = new();
    private readonly BrevoFormModel _brevoForm = new();
    private EditContext _editContext = default!;
    private EditContext _brevoEditContext = default!;
    private bool _isCheckingSetup = true;
    private bool _isSetupRequired;
    private bool _isSubmitting;
    private bool _brevoSkipped;
    private string? _errorMessage;

    private SetupStep CurrentStep { get; set; } = SetupStep.Account;

    [Inject]
    private IInitialSetupService InitialSetupService { get; set; } = default!;

    [Inject]
    private IEmailProviderConfigService EmailProviderConfigService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IStringLocalizer<SetupResource> L { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_form);
        _brevoEditContext = new EditContext(_brevoForm);
        _isSetupRequired = await InitialSetupService.IsSetupRequiredAsync(_cts.Token).ConfigureAwait(false);
        _isCheckingSetup = false;

        if (!_isSetupRequired)
        {
            NavigationManager.NavigateTo("/", replace: true);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private Task GoToEmailProviderStepAsync()
    {
        _errorMessage = null;
        CurrentStep = SetupStep.EmailProvider;
        return Task.CompletedTask;
    }

    private Task GoToConfirmationStepAsync()
    {
        _errorMessage = null;
        _brevoSkipped = false;
        CurrentStep = SetupStep.Confirmation;
        return Task.CompletedTask;
    }

    private void SkipEmailProviderStep()
    {
        _errorMessage = null;
        _brevoSkipped = true;
        CurrentStep = SetupStep.Confirmation;
    }

    private void GoToAccountStep()
    {
        _errorMessage = null;
        CurrentStep = SetupStep.Account;
    }

    private void GoToEmailProviderStep()
    {
        _errorMessage = null;
        CurrentStep = SetupStep.EmailProvider;
    }

    private async Task CompleteSetupAsync()
    {
        _isSubmitting = true;
        _errorMessage = null;

        Result accountResult = await CreateAdminAccountAsync();
        if (accountResult.IsFailure)
        {
            _errorMessage = L.LocalizeError(accountResult.Error!);
            _isSubmitting = false;
            return;
        }

        if (!_brevoSkipped)
        {
            Result emailResult = await SaveEmailProviderConfigAsync();
            if (emailResult.IsFailure)
            {
                _errorMessage = L.LocalizeError(emailResult.Error!);
                _isSubmitting = false;
                return;
            }
        }

        NavigationManager.NavigateTo("/", replace: true);
    }

    private async Task<Result> CreateAdminAccountAsync()
    {
        CompleteInitialSetupRequest request = new(
            _form.UserName,
            _form.Email,
            _form.Password);

        return await InitialSetupService
            .CompleteInitialSetupAsync(request, _cts.Token)
            .ConfigureAwait(false);
    }

    private async Task<Result> SaveEmailProviderConfigAsync()
    {
        SaveEmailProviderConfigRequest request = new(
            _brevoForm.ApiKey,
            _brevoForm.SenderEmail,
            _brevoForm.SenderName);

        Result<EmailProviderConfigDto> result =
            await EmailProviderConfigService.SaveAsync(request, _cts.Token).ConfigureAwait(false);

        return result.IsFailure
            ? Result.Failure(result.Error!)
            : Result.Success();
    }

    private enum SetupStep
    {
        Account,
        EmailProvider,
        Confirmation,
    }

    private sealed class SetupFormModel : IValidatableObject
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                yield return new ValidationResult("setup.user_name_required", [nameof(UserName)]);
            }
            else if (UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
            {
                yield return new ValidationResult("setup.user_name_too_long", [nameof(UserName)]);
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("setup.email_required", [nameof(Email)]);
            }
            else if (!EmailValidator.IsValid(Email.Trim()))
            {
                yield return new ValidationResult("setup.invalid_email", [nameof(Email)]);
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("setup.password_required", [nameof(Password)]);
            }
            else if (Password.Trim().Length < ValidationConstants.PasswordMinLength)
            {
                yield return new ValidationResult("setup.password_too_short", [nameof(Password)]);
            }
        }
    }

    private sealed class BrevoFormModel : IValidatableObject
    {
        public string ApiKey { get; set; } = string.Empty;

        public string SenderEmail { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                yield return new ValidationResult("setup.brevo.api_key_required", [nameof(ApiKey)]);
            }

            if (string.IsNullOrWhiteSpace(SenderEmail))
            {
                yield return new ValidationResult("setup.brevo.sender_email_required", [nameof(SenderEmail)]);
            }
            else if (!EmailValidator.IsValid(SenderEmail.Trim()))
            {
                yield return new ValidationResult("setup.brevo.sender_email_invalid", [nameof(SenderEmail)]);
            }

            if (string.IsNullOrWhiteSpace(SenderName))
            {
                yield return new ValidationResult("setup.brevo.sender_name_required", [nameof(SenderName)]);
            }
        }
    }
}
