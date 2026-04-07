// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using Beagl.Application.EmailProviders.Services;
using Beagl.Domain.EmailProviders;
using Beagl.Domain.Results;
using Microsoft.Extensions.Logging;

namespace Beagl.Infrastructure.EmailProviders;

/// <summary>
/// Sends transactional emails through the Brevo (Sendinblue) API.
/// </summary>
/// <param name="httpClientFactory">The HTTP client factory.</param>
/// <param name="emailProviderConfigRepository">The email provider configuration repository.</param>
/// <param name="logger">The logger.</param>
public sealed partial class BrevoEmailSender(
    IHttpClientFactory httpClientFactory,
    IEmailProviderConfigRepository emailProviderConfigRepository,
    ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private readonly IEmailProviderConfigRepository _emailProviderConfigRepository =
        emailProviderConfigRepository ?? throw new ArgumentNullException(nameof(emailProviderConfigRepository));

    private readonly ILogger<BrevoEmailSender> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<Result> SendAsync(
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken)
    {
        EmailProviderConfig? config = await _emailProviderConfigRepository
            .GetActiveAsync(cancellationToken)
            .ConfigureAwait(false);

        if (config is null)
        {
            return Result.Failure(
                new ResultError("email.no_provider_configured", "No email provider is configured."));
        }

        BrevoEmailRequest request = new()
        {
            Sender = new BrevoContact { Email = config.SenderEmail, Name = config.SenderName },
            To = [new BrevoContact { Email = recipientEmail, Name = recipientName }],
            Subject = subject,
            HtmlContent = htmlContent,
        };

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);

        try
        {
            using HttpResponseMessage response = await httpClient
                .PostAsJsonAsync(BrevoApiUrl, request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                LogSendFailed(_logger, recipientEmail, (int)response.StatusCode, body);

                return Result.Failure(
                    new ResultError("email.send_failed", "The email could not be sent."));
            }

            LogEmailSent(_logger, recipientEmail);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            LogSendException(_logger, recipientEmail, ex);
            return Result.Failure(
                new ResultError("email.send_failed", "The email could not be sent."));
        }
    }

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Email sent to {RecipientEmail}")]
    private static partial void LogEmailSent(ILogger logger, string recipientEmail);

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 3002, Level = LogLevel.Error, Message = "Failed to send email to {RecipientEmail}: HTTP {StatusCode} - {ResponseBody}")]
    private static partial void LogSendFailed(ILogger logger, string recipientEmail, int statusCode, string responseBody);

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 3003, Level = LogLevel.Error, Message = "Exception sending email to {RecipientEmail}")]
    private static partial void LogSendException(ILogger logger, string recipientEmail, Exception exception);

    private sealed class BrevoEmailRequest
    {
        [JsonPropertyName("sender")]
        public BrevoContact Sender { get; set; } = null!;

        [JsonPropertyName("to")]
        public List<BrevoContact> To { get; set; } = [];

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; set; } = string.Empty;
    }

    private sealed class BrevoContact
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
