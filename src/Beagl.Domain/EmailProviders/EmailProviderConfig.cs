// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.EmailProviders;

/// <summary>
/// Represents the active email provider configuration for the application.
/// </summary>
/// <param name="Id">The unique configuration identifier.</param>
/// <param name="ApiKey">The provider API key.</param>
/// <param name="SenderEmail">The sender email address.</param>
/// <param name="SenderName">The sender display name.</param>
public sealed record EmailProviderConfig(
    Guid Id,
    string ApiKey,
    string SenderEmail,
    string SenderName);
