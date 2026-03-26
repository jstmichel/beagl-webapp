// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.EmailProviders.Dtos;
using Beagl.Domain.Results;

namespace Beagl.Application.EmailProviders.Services;

/// <summary>
/// Provides management operations for the email provider configuration.
/// </summary>
public interface IEmailProviderConfigService
{
    /// <summary>
    /// Gets the current active email provider configuration, if any.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The active configuration DTO, or <see langword="null"/> if none exists.</returns>
    public Task<EmailProviderConfigDto?> GetActiveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates the email provider configuration.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The saved configuration DTO, or a validation failure.</returns>
    public Task<Result<EmailProviderConfigDto>> SaveAsync(SaveEmailProviderConfigRequest request, CancellationToken cancellationToken);
}
