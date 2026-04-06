// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Collections.Frozen;
using System.Reflection;
using Beagl.Domain.Users;

namespace Beagl.Application.EmailProviders.Services;

/// <summary>
/// Renders localized HTML email templates from embedded resources with token replacement.
/// </summary>
public sealed class EmailTemplateService : IEmailTemplateService
{
    private const string _templateNamespace = "Beagl.Application.EmailProviders.Templates";

    private static readonly FrozenDictionary<string, string> _subjects = new Dictionary<string, string>
    {
        ["EmailConfirmation_en"] = "Confirm your email address",
        ["EmailConfirmation_fr"] = "Confirmez votre adresse courriel",
        ["RecoveryCode_en"] = "Your account recovery code",
        ["RecoveryCode_fr"] = "Votre code de récupération de compte",
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, string> _templates = LoadAllTemplates();

    /// <inheritdoc />
    public EmailTemplateResult RenderEmailConfirmation(
        LanguagePreference languagePreference,
        EmailConfirmationTokens tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        string lang = ResolveLanguage(languagePreference);
        string key = $"EmailConfirmation_{lang}";
        string subject = _subjects[key];
        string template = _templates[key];

        string html = template
            .Replace("{{UserName}}", tokens.UserName, StringComparison.Ordinal)
            .Replace("{{ConfirmationLink}}", tokens.ConfirmationLink, StringComparison.Ordinal);

        return new EmailTemplateResult(subject, html);
    }

    /// <inheritdoc />
    public EmailTemplateResult RenderRecoveryCode(
        LanguagePreference languagePreference,
        RecoveryCodeTokens tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        string lang = ResolveLanguage(languagePreference);
        string key = $"RecoveryCode_{lang}";
        string subject = _subjects[key];
        string template = _templates[key];

        string html = template
            .Replace("{{UserName}}", tokens.UserName, StringComparison.Ordinal)
            .Replace("{{RecoveryCode}}", tokens.RecoveryCode, StringComparison.Ordinal);

        return new EmailTemplateResult(subject, html);
    }

    private static string ResolveLanguage(LanguagePreference preference)
    {
        return preference == LanguagePreference.French ? "fr" : "en";
    }

    private static FrozenDictionary<string, string> LoadAllTemplates()
    {
        Assembly assembly = typeof(EmailTemplateService).Assembly;
        Dictionary<string, string> templates = new(StringComparer.Ordinal);

        string[] templateNames =
        [
            "EmailConfirmation_en",
            "EmailConfirmation_fr",
            "RecoveryCode_en",
            "RecoveryCode_fr",
        ];

        foreach (string name in templateNames)
        {
            string resourceName = $"{_templateNamespace}.{name}.html";
            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded email template '{resourceName}' not found.");

            using StreamReader reader = new(stream);
            templates[name] = reader.ReadToEnd();
        }

        return templates.ToFrozenDictionary();
    }
}
