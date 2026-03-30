// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;

namespace Beagl.WebApp.Extensions;

/// <summary>
/// Provides reusable email address validation using a shared <see cref="EmailAddressAttribute"/> instance.
/// </summary>
internal static class EmailValidator
{
    private static readonly EmailAddressAttribute _emailAttribute = new();

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="email"/> is a syntactically valid email address.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns><see langword="true"/> if the email is valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(string? email)
    {
        return _emailAttribute.IsValid(email);
    }
}
