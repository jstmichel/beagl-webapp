// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Application.EmailProviders.Dtos;
using Beagl.Domain.EmailProviders;
using Beagl.Domain.Results;

namespace Beagl.Application.EmailProviders.Services;

/// <summary>
/// Implements management operations for the email provider configuration.
/// </summary>
/// <param name="repository">The email provider configuration repository.</param>
public sealed class EmailProviderConfigService(IEmailProviderConfigRepository repository) : IEmailProviderConfigService
{
    private static readonly EmailAddressAttribute _emailValidator = new();
    private readonly IEmailProviderConfigRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <inheritdoc />
    public async Task<EmailProviderConfigDto?> GetActiveAsync(CancellationToken cancellationToken)
    {
        EmailProviderConfig? config = await _repository.GetActiveAsync(cancellationToken).ConfigureAwait(false);
        return config is null ? null : MapToDto(config);
    }

    /// <inheritdoc />
    public async Task<Result<EmailProviderConfigDto>> SaveAsync(SaveEmailProviderConfigRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<EmailProviderConfigDto>(validation.Error!);
        }

        EmailProviderConfig? existing = await _repository.GetActiveAsync(cancellationToken).ConfigureAwait(false);

        EmailProviderConfig config = new(
            existing?.Id ?? Guid.NewGuid(),
            request.ApiKey.Trim(),
            request.SenderEmail.Trim(),
            request.SenderName.Trim());

        EmailProviderConfig saved = await _repository.SaveAsync(config, cancellationToken).ConfigureAwait(false);
        return Result.Success(MapToDto(saved));
    }

    private static Result ValidateRequest(SaveEmailProviderConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return Result.Failure(new ResultError("email_config.api_key_required", "The API key is required."));
        }

        if (string.IsNullOrWhiteSpace(request.SenderEmail))
        {
            return Result.Failure(new ResultError("email_config.sender_email_required", "The sender email address is required."));
        }

        if (!_emailValidator.IsValid(request.SenderEmail.Trim()))
        {
            return Result.Failure(new ResultError("email_config.sender_email_invalid", "The sender email address is not valid."));
        }

        if (string.IsNullOrWhiteSpace(request.SenderName))
        {
            return Result.Failure(new ResultError("email_config.sender_name_required", "The sender name is required."));
        }

        return Result.Success();
    }

    private static EmailProviderConfigDto MapToDto(EmailProviderConfig config)
    {
        return new EmailProviderConfigDto(config.Id, MaskApiKey(config.ApiKey), config.SenderEmail, config.SenderName);
    }

    private static string MaskApiKey(string apiKey)
    {
        if (apiKey.Length <= 4)
        {
            return new string('*', apiKey.Length);
        }

        return string.Concat(new string('*', apiKey.Length - 4), apiKey.AsSpan(apiKey.Length - 4));
    }
}
