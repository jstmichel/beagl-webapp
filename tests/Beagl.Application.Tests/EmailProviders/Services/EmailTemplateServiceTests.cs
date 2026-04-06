// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.EmailProviders.Services;
using Beagl.Domain.Users;
using FluentAssertions;

namespace Beagl.Application.Tests.EmailProviders.Services;

public class EmailTemplateServiceTests
{
    private readonly EmailTemplateService _service = new();

    // ── RenderEmailConfirmation ─────────────────────────────────────────────

    [Fact]
    public void RenderEmailConfirmation_WithNullTokens_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _service.RenderEmailConfirmation(LanguagePreference.English, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RenderEmailConfirmation_WithEnglishPreference_ShouldReturnEnglishSubject()
    {
        // Arrange
        EmailConfirmationTokens tokens = new("John", "https://example.com/confirm?token=abc");

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.English, tokens);

        // Assert
        result.Subject.Should().Be("Confirm your email address");
    }

    [Fact]
    public void RenderEmailConfirmation_WithFrenchPreference_ShouldReturnFrenchSubject()
    {
        // Arrange
        EmailConfirmationTokens tokens = new("Jean", "https://example.com/confirm?token=abc");

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.French, tokens);

        // Assert
        result.Subject.Should().Be("Confirmez votre adresse courriel");
    }

    [Fact]
    public void RenderEmailConfirmation_WithNonePreference_ShouldFallBackToEnglish()
    {
        // Arrange
        EmailConfirmationTokens tokens = new("John", "https://example.com/confirm?token=abc");

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.None, tokens);

        // Assert
        result.Subject.Should().Be("Confirm your email address");
        result.HtmlBody.Should().Contain("Please confirm your email address");
    }

    [Fact]
    public void RenderEmailConfirmation_ShouldReplaceUserNameToken()
    {
        // Arrange
        EmailConfirmationTokens tokens = new("Alice", "https://example.com/confirm");

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain("Alice");
        result.HtmlBody.Should().NotContain("{{UserName}}");
    }

    [Fact]
    public void RenderEmailConfirmation_ShouldReplaceConfirmationLinkToken()
    {
        // Arrange
        string link = "https://example.com/confirm?userId=user-1&token=abc123";
        EmailConfirmationTokens tokens = new("John", link);

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain(link);
        result.HtmlBody.Should().NotContain("{{ConfirmationLink}}");
    }

    [Fact]
    public void RenderEmailConfirmation_WithFrenchPreference_ShouldReturnFrenchBody()
    {
        // Arrange
        EmailConfirmationTokens tokens = new("Jean", "https://example.com/confirm");

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.French, tokens);

        // Assert
        result.HtmlBody.Should().Contain("Bonjour Jean");
        result.HtmlBody.Should().Contain("Confirmer mon adresse courriel");
    }

    [Fact]
    public void RenderEmailConfirmation_WithEnglishPreference_ShouldReturnEnglishBody()
    {
        // Arrange
        EmailConfirmationTokens tokens = new("John", "https://example.com/confirm");

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain("Hello John");
        result.HtmlBody.Should().Contain("Confirm my email address");
    }

    // ── RenderRecoveryCode ──────────────────────────────────────────────────

    [Fact]
    public void RenderRecoveryCode_WithNullTokens_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _service.RenderRecoveryCode(LanguagePreference.English, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RenderRecoveryCode_WithEnglishPreference_ShouldReturnEnglishSubject()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("John", "ABC123");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.English, tokens);

        // Assert
        result.Subject.Should().Be("Your account recovery code");
    }

    [Fact]
    public void RenderRecoveryCode_WithFrenchPreference_ShouldReturnFrenchSubject()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("Jean", "ABC123");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.French, tokens);

        // Assert
        result.Subject.Should().Be("Votre code de récupération de compte");
    }

    [Fact]
    public void RenderRecoveryCode_WithNonePreference_ShouldFallBackToEnglish()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("John", "ABC123");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.None, tokens);

        // Assert
        result.Subject.Should().Be("Your account recovery code");
        result.HtmlBody.Should().Contain("Your account recovery code");
    }

    [Fact]
    public void RenderRecoveryCode_ShouldReplaceUserNameToken()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("Alice", "XYZ789");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain("Alice");
        result.HtmlBody.Should().NotContain("{{UserName}}");
    }

    [Fact]
    public void RenderRecoveryCode_ShouldReplaceRecoveryCodeToken()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("John", "XYZ789");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain("XYZ789");
        result.HtmlBody.Should().NotContain("{{RecoveryCode}}");
    }

    [Fact]
    public void RenderRecoveryCode_WithFrenchPreference_ShouldReturnFrenchBody()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("Jean", "ABC123");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.French, tokens);

        // Assert
        result.HtmlBody.Should().Contain("Bonjour Jean");
        result.HtmlBody.Should().Contain("ABC123");
    }

    [Fact]
    public void RenderRecoveryCode_WithEnglishPreference_ShouldReturnEnglishBody()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("John", "ABC123");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain("Hello John");
        result.HtmlBody.Should().Contain("ABC123");
    }

    [Fact]
    public void RenderEmailConfirmation_ShouldReturnValidHtml()
    {
        // Arrange
        EmailConfirmationTokens tokens = new("John", "https://example.com/confirm");

        // Act
        EmailTemplateResult result = _service.RenderEmailConfirmation(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain("<!DOCTYPE html>");
        result.HtmlBody.Should().Contain("<html");
        result.HtmlBody.Should().Contain("</html>");
    }

    [Fact]
    public void RenderRecoveryCode_ShouldReturnValidHtml()
    {
        // Arrange
        RecoveryCodeTokens tokens = new("John", "ABC123");

        // Act
        EmailTemplateResult result = _service.RenderRecoveryCode(LanguagePreference.English, tokens);

        // Assert
        result.HtmlBody.Should().Contain("<!DOCTYPE html>");
        result.HtmlBody.Should().Contain("<html");
        result.HtmlBody.Should().Contain("</html>");
    }
}
