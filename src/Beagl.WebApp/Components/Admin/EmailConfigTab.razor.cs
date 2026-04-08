// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.EmailProviders.Dtos;
using Beagl.Application.EmailProviders.Services;
using Beagl.Domain.Results;
using Beagl.WebApp.Extensions;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Admin;

/// <summary>
/// Code-behind for the email provider configuration tab component.
/// </summary>
public sealed partial class EmailConfigTab : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<string> _formErrors = [];
    private readonly EmailConfigFormModel _form = new();
    private EmailProviderConfigDto? _config;
    private bool _isLoading = true;
    private bool _showForm;
    private bool _isSubmitting;
    private string? _statusMessage;
    private string? _errorMessage;

    [Inject]
    private IEmailProviderConfigService EmailProviderConfigService { get; set; } = default!;

    [Inject]
    private IStringLocalizer<EmailConfigResource> L { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await LoadConfigAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private async Task LoadConfigAsync()
    {
        _isLoading = true;
        _config = await EmailProviderConfigService.GetActiveAsync(_cts.Token).ConfigureAwait(false);
        _isLoading = false;
    }

    private void OpenCreateForm()
    {
        _form.Reset();
        _showForm = true;
        _statusMessage = null;
        _errorMessage = null;
    }

    private void OpenEditForm()
    {
        if (_config is not null)
        {
            _form.SenderEmail = _config.SenderEmail;
            _form.SenderName = _config.SenderName;
            _form.ApiKey = string.Empty;
        }

        _showForm = true;
        _statusMessage = null;
        _errorMessage = null;
        _formErrors.Clear();
    }

    private void CancelEdit()
    {
        _showForm = false;
        _formErrors.Clear();
    }

    private async Task SaveAsync()
    {
        _isSubmitting = true;
        _formErrors.Clear();

        SaveEmailProviderConfigRequest request = new(
            _form.ApiKey,
            _form.SenderEmail,
            _form.SenderName);

        Result<EmailProviderConfigDto> result =
            await EmailProviderConfigService.SaveAsync(request, _cts.Token).ConfigureAwait(false);

        if (result.IsFailure)
        {
            _formErrors.Add(L.LocalizeError(result.Error!));
            _isSubmitting = false;
            return;
        }

        _config = result.Value;
        _showForm = false;
        _statusMessage = L["EmailConfig.SaveSuccess"];
        _isSubmitting = false;
    }

    private sealed class EmailConfigFormModel
    {
        public string ApiKey { get; set; } = string.Empty;

        public string SenderEmail { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public void Reset()
        {
            ApiKey = string.Empty;
            SenderEmail = string.Empty;
            SenderName = string.Empty;
        }
    }
}
