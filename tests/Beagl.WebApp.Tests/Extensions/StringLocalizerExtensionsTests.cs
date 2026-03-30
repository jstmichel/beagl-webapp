// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;
using Beagl.WebApp.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace Beagl.WebApp.Tests.Extensions;

public sealed class StringLocalizerExtensionsTests
{
    private readonly Mock<IStringLocalizer> _localizerMock = new();

    [Fact]
    public void LocalizeError_ShouldReturnLocalizedValue_WhenResourceExists()
    {
        ResultError error = new("users.not_found", "The requested user could not be found.");
        LocalizedString localized = new("users.not_found", "L'utilisateur demandé est introuvable.");
        _localizerMock.Setup(l => l["users.not_found"]).Returns(localized);

        string result = _localizerMock.Object.LocalizeError(error);

        result.Should().Be("L'utilisateur demandé est introuvable.");
    }

    [Fact]
    public void LocalizeError_ShouldFallBackToErrorMessage_WhenResourceNotFound()
    {
        ResultError error = new("users.not_found", "The requested user could not be found.");
        LocalizedString notFound = new("users.not_found", "users.not_found", resourceNotFound: true);
        _localizerMock.Setup(l => l["users.not_found"]).Returns(notFound);

        string result = _localizerMock.Object.LocalizeError(error);

        result.Should().Be("The requested user could not be found.");
    }

    [Fact]
    public void LocalizeError_ShouldThrow_WhenLocalizerIsNull()
    {
        ResultError error = new("code", "message");

        Action act = () => ((IStringLocalizer)null!).LocalizeError(error);

        act.Should().Throw<ArgumentNullException>().WithParameterName("localizer");
    }

    [Fact]
    public void LocalizeError_ShouldThrow_WhenErrorIsNull()
    {
        Action act = () => _localizerMock.Object.LocalizeError(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }

    [Fact]
    public void LocalizeValidationMessage_ShouldReturnLocalizedValue_WhenResourceExists()
    {
        LocalizedString localized = new("users.email_required", "Le courriel est requis.");
        _localizerMock.Setup(l => l["users.email_required"]).Returns(localized);

        string result = _localizerMock.Object.LocalizeValidationMessage("users.email_required");

        result.Should().Be("Le courriel est requis.");
    }

    [Fact]
    public void LocalizeValidationMessage_ShouldFallBackToOriginalMessage_WhenResourceNotFound()
    {
        LocalizedString notFound = new("users.email_required", "users.email_required", resourceNotFound: true);
        _localizerMock.Setup(l => l["users.email_required"]).Returns(notFound);

        string result = _localizerMock.Object.LocalizeValidationMessage("users.email_required");

        result.Should().Be("users.email_required");
    }

    [Fact]
    public void LocalizeValidationMessage_ShouldThrow_WhenLocalizerIsNull()
    {
        Action act = () => ((IStringLocalizer)null!).LocalizeValidationMessage("msg");

        act.Should().Throw<ArgumentNullException>().WithParameterName("localizer");
    }

    [Fact]
    public void LocalizeValidationMessage_ShouldThrow_WhenMessageIsNull()
    {
        Action act = () => _localizerMock.Object.LocalizeValidationMessage(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("message");
    }
}
