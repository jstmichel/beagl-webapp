// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;

namespace Beagl.Application.EmailProviders.Services;

/// <summary>
/// Renders localized HTML email templates with token replacement.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders the email confirmation template with the specified tokens.
    /// </summary>
    /// <param name="languagePreference">The recipient's language preference.</param>
    /// <param name="tokens">The token values to replace in the template.</param>
    /// <returns>The rendered subject and HTML body.</returns>
    public EmailTemplateResult RenderEmailConfirmation(
        LanguagePreference languagePreference,
        EmailConfirmationTokens tokens);

    /// <summary>
    /// Renders the recovery code template with the specified tokens.
    /// </summary>
    /// <param name="languagePreference">The recipient's language preference.</param>
    /// <param name="tokens">The token values to replace in the template.</param>
    /// <returns>The rendered subject and HTML body.</returns>
    public EmailTemplateResult RenderRecoveryCode(
        LanguagePreference languagePreference,
        RecoveryCodeTokens tokens);
}

/// <summary>
/// Represents the rendered output of an email template.
/// </summary>
/// <param name="Subject">The email subject line.</param>
/// <param name="HtmlBody">The rendered HTML body.</param>
public sealed record EmailTemplateResult(string Subject, string HtmlBody);

/// <summary>
/// Token values for the email confirmation template.
/// </summary>
/// <param name="UserName">The recipient's display name.</param>
/// <param name="ConfirmationLink">The full confirmation URL.</param>
public sealed record EmailConfirmationTokens(string UserName, string ConfirmationLink);

/// <summary>
/// Token values for the recovery code template.
/// </summary>
/// <param name="UserName">The recipient's display name.</param>
/// <param name="RecoveryCode">The recovery code value.</param>
public sealed record RecoveryCodeTokens(string UserName, string RecoveryCode);
