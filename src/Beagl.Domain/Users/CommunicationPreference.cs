// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Defines the supported communication preference options for citizen profiles.
/// </summary>
public enum CommunicationPreference
{
    /// <summary>
    /// No preference selected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Prefers communication by email.
    /// </summary>
    Email = 1,

    /// <summary>
    /// Prefers communication by phone.
    /// </summary>
    Phone = 2,

    /// <summary>
    /// Prefers communication by mail.
    /// </summary>
    Mail = 3,
}
