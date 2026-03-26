// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.EmailProviders.Dtos;

/// <summary>
/// Represents the email provider configuration returned to the UI, with the API key masked.
/// </summary>
/// <param name="Id">The unique configuration identifier.</param>
/// <param name="MaskedApiKey">The masked API key showing only the last four characters.</param>
/// <param name="SenderEmail">The sender email address.</param>
/// <param name="SenderName">The sender display name.</param>
public sealed record EmailProviderConfigDto(
    Guid Id,
    string MaskedApiKey,
    string SenderEmail,
    string SenderName);
