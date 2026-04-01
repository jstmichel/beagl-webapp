// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Users;

/// <summary>
/// Defines the supported language preference options for citizen profiles.
/// </summary>
public enum LanguagePreference
{
    /// <summary>
    /// No preference selected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Prefers communication in English.
    /// </summary>
    English = 1,

    /// <summary>
    /// Prefers communication in French.
    /// </summary>
    French = 2,
}
