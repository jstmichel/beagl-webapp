// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Services;
using Beagl.Domain.Results;
using Beagl.WebApp.Pages.Account;
using Beagl.WebApp.Resources;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;
using System.Globalization;

namespace Beagl.WebApp.Tests.Pages.Account;

public sealed class ConfirmEmailModelTests
{
    [Fact]
    public async Task OnGetAsync_WithMissingUserId_ShouldShowError()
    {
        // Arrange
        ConfirmEmailModel model = CreateModel(out Mock<IUserManagementService> _);

        // Act
        IActionResult result = await model.OnGetAsync(null, "some-token");

        // Assert
        result.Should().BeOfType<PageResult>();
        model.IsError.Should().BeTrue();
        model.IsSuccess.Should().BeFalse();
        model.StatusMessage.Should().Be("Auth.ConfirmEmail.Error.MissingToken");
    }

    [Fact]
    public async Task OnGetAsync_WithMissingToken_ShouldShowError()
    {
        // Arrange
        ConfirmEmailModel model = CreateModel(out Mock<IUserManagementService> _);

        // Act
        IActionResult result = await model.OnGetAsync("user-1", null);

        // Assert
        result.Should().BeOfType<PageResult>();
        model.IsError.Should().BeTrue();
        model.IsSuccess.Should().BeFalse();
        model.StatusMessage.Should().Be("Auth.ConfirmEmail.Error.MissingToken");
    }

    [Fact]
    public async Task OnGetAsync_WithEmptyToken_ShouldShowError()
    {
        // Arrange
        ConfirmEmailModel model = CreateModel(out Mock<IUserManagementService> _);

        // Act
        IActionResult result = await model.OnGetAsync("user-1", " ");

        // Assert
        result.Should().BeOfType<PageResult>();
        model.IsError.Should().BeTrue();
        model.StatusMessage.Should().Be("Auth.ConfirmEmail.Error.MissingToken");
    }

    [Fact]
    public async Task OnGetAsync_WhenConfirmationSucceeds_ShouldShowSuccess()
    {
        // Arrange
        ConfirmEmailModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        serviceMock
            .Setup(service => service.ConfirmAccountByTokenAsync("user-1", "valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        IActionResult result = await model.OnGetAsync("user-1", "valid-token");

        // Assert
        result.Should().BeOfType<PageResult>();
        model.IsSuccess.Should().BeTrue();
        model.IsError.Should().BeFalse();
        model.StatusMessage.Should().Be("Auth.ConfirmEmail.Success");
    }

    [Fact]
    public async Task OnGetAsync_WhenConfirmationFails_ShouldShowError()
    {
        // Arrange
        ConfirmEmailModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        serviceMock
            .Setup(service => service.ConfirmAccountByTokenAsync("user-1", "bad-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.invalid_token", "The confirmation token is invalid or has expired.")));

        // Act
        IActionResult result = await model.OnGetAsync("user-1", "bad-token");

        // Assert
        result.Should().BeOfType<PageResult>();
        model.IsError.Should().BeTrue();
        model.IsSuccess.Should().BeFalse();
        model.StatusMessage.Should().Be("Auth.ConfirmEmail.Error.Failed");
    }

    [Fact]
    public async Task OnGetAsync_ShouldNotCallService_WhenParametersAreMissing()
    {
        // Arrange
        ConfirmEmailModel model = CreateModel(out Mock<IUserManagementService> serviceMock);

        // Act
        await model.OnGetAsync(null, null);

        // Assert
        serviceMock.Verify(
            service => service.ConfirmAccountByTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static ConfirmEmailModel CreateModel(out Mock<IUserManagementService> serviceMock)
    {
        serviceMock = new Mock<IUserManagementService>();
        TestLocalizer localizer = new();

        ConfirmEmailModel model = new(serviceMock.Object, localizer)
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

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return [];
        }
    }
}
