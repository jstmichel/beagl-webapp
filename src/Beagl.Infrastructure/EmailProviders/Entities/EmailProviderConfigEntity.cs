// MIT License - Copyright (c) 2025 Jonathan St-Michele

using System.Diagnostics.CodeAnalysis;

namespace Beagl.Infrastructure.EmailProviders.Entities;

/// <summary>
/// Represents the persisted email provider configuration row.
/// </summary>
[ExcludeFromCodeCoverage]
public class EmailProviderConfigEntity
{
    /// <summary>
    /// Gets or sets the unique configuration identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the provider API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender display name.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
}
