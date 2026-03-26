// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;

namespace Beagl.Domain.EmailProviders;

/// <summary>
/// Provides persistence operations for the email provider configuration.
/// </summary>
public interface IEmailProviderConfigRepository
{
    /// <summary>
    /// Gets the current active email provider configuration, if any.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The active configuration, or <see langword="null"/> if none exists.</returns>
    public Task<EmailProviderConfig?> GetActiveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Persists the email provider configuration.
    /// Creates a new record when none exists; otherwise updates the existing one.
    /// </summary>
    /// <param name="config">The configuration to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The persisted configuration.</returns>
    public Task<EmailProviderConfig> SaveAsync(EmailProviderConfig config, CancellationToken cancellationToken);
}
