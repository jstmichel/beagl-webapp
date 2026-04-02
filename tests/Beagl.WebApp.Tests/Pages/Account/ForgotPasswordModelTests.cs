// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Services;
using Beagl.WebApp.Pages.Account;
using Beagl.WebApp.Resources;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;
using System.Globalization;

namespace Beagl.WebApp.Tests.Pages.Account;

public sealed class ForgotPasswordModelTests
{
    [Fact]
    public void OnGet_ShouldLeaveModelStateValid()
    {
        ForgotPasswordModel model = CreateModel(out _);

        model.OnGet();

        model.ModelState.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_WhenIdentifierIsNull_ShouldReturnPageWithModelError()
    {
        // Arrange
        ForgotPasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ForgotPasswordModel.ForgotPasswordInputModel { Identifier = null };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(ForgotPasswordModel.ForgotPasswordInputModel.Identifier));
        serviceMock.Verify(
            service => service.RequestRecoveryCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdentifierIsEmpty_ShouldReturnPageWithModelError()
    {
        // Arrange
        ForgotPasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ForgotPasswordModel.ForgotPasswordInputModel { Identifier = string.Empty };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(ForgotPasswordModel.ForgotPasswordInputModel.Identifier));
        serviceMock.Verify(
            service => service.RequestRecoveryCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdentifierIsWhitespace_ShouldReturnPageWithModelError()
    {
        // Arrange
        ForgotPasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ForgotPasswordModel.ForgotPasswordInputModel { Identifier = "   " };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(ForgotPasswordModel.ForgotPasswordInputModel.Identifier));
        serviceMock.Verify(
            service => service.RequestRecoveryCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdentifierIsValid_ShouldCallService()
    {
        // Arrange
        ForgotPasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ForgotPasswordModel.ForgotPasswordInputModel { Identifier = "alex" };
        serviceMock
            .Setup(service => service.RequestRecoveryCodeAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Beagl.Domain.Results.Result.Success());

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.IsSubmitted.Should().BeTrue();
        serviceMock.Verify(
            service => service.RequestRecoveryCodeAsync("alex", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdentifierIsValid_ShouldSetIsSubmittedTrue()
    {
        // Arrange
        ForgotPasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ForgotPasswordModel.ForgotPasswordInputModel { Identifier = "employee1" };
        serviceMock
            .Setup(service => service.RequestRecoveryCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Beagl.Domain.Results.Result.Success());

        // Act
        await model.OnPostAsync();

        // Assert
        model.IsSubmitted.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_WhenIdentifierIsMissing_ShouldNotSetIsSubmitted()
    {
        // Arrange
        ForgotPasswordModel model = CreateModel(out _);
        model.Input = new ForgotPasswordModel.ForgotPasswordInputModel { Identifier = null };

        // Act
        await model.OnPostAsync();

        // Assert
        model.IsSubmitted.Should().BeFalse();
    }

    private static ForgotPasswordModel CreateModel(
        out Mock<IUserManagementService> serviceMock)
    {
        serviceMock = new Mock<IUserManagementService>();

        ForgotPasswordModel model = new(serviceMock.Object, new TestLocalizer())
        {
            PageContext = new PageContext(
                new ActionContext(
                    new DefaultHttpContext(),
                    new RouteData(),
                    new ActionDescriptor(),
                    new ModelStateDictionary())),
        };

        return model;
    }

    private sealed class TestLocalizer : IStringLocalizer<AuthResource>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
