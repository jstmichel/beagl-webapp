// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.EmailProviders.Dtos;

/// <summary>
/// Represents the request payload used to create or update the email provider configuration.
/// </summary>
/// <param name="ApiKey">The provider API key.</param>
/// <param name="SenderEmail">The sender email address.</param>
/// <param name="SenderName">The sender display name.</param>
public sealed record SaveEmailProviderConfigRequest(
    string ApiKey,
    string SenderEmail,
    string SenderName);
