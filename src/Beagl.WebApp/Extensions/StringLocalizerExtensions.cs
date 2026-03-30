// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Extensions;

/// <summary>
/// Extension methods for <see cref="IStringLocalizer"/> that simplify error and validation localization.
/// </summary>
internal static class StringLocalizerExtensions
{
    /// <summary>
    /// Returns a localized message for the given <see cref="ResultError"/>,
    /// falling back to the error's default message when no resource is found.
    /// </summary>
    /// <param name="localizer">The string localizer.</param>
    /// <param name="error">The result error to localize.</param>
    /// <returns>The localized error message.</returns>
    public static string LocalizeError(this IStringLocalizer localizer, ResultError error)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(error);

        LocalizedString localizedError = localizer[error.Code];
        return localizedError.ResourceNotFound ? error.Message : localizedError.Value;
    }

    /// <summary>
    /// Returns a localized validation message, falling back to the original message
    /// when no matching resource is found.
    /// </summary>
    /// <param name="localizer">The string localizer.</param>
    /// <param name="message">The validation message key.</param>
    /// <returns>The localized validation message.</returns>
    public static string LocalizeValidationMessage(this IStringLocalizer localizer, string message)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(message);

        LocalizedString localizedMessage = localizer[message];
        return localizedMessage.ResourceNotFound ? message : localizedMessage.Value;
    }
}
