// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;

namespace Beagl.Application.EmailProviders.Services;

/// <summary>
/// Sends transactional emails through the configured email provider.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends a transactional email.
    /// </summary>
    /// <param name="recipientEmail">The recipient email address.</param>
    /// <param name="recipientName">The recipient display name.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="htmlContent">The email body in HTML format.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the send operation.</returns>
    public Task<Result> SendAsync(
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken);
}
