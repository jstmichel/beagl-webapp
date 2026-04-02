// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Dtos;
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
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;
using System.Globalization;

namespace Beagl.WebApp.Tests.Pages.Account;

public sealed class RecoverAccountModelTests
{
    [Fact]
    public void OnGet_ShouldLeaveModelStateValid()
    {
        RecoverAccountModel model = CreateModel(out _);

        model.OnGet();

        model.ModelState.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_WhenCodeIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        RecoverAccountModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = null,
            NewPassword = "StrongPassword!1",
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey($"{nameof(RecoverAccountModel.Input)}.{nameof(RecoverAccountModel.RecoverAccountInputModel.Code)}");
        serviceMock.Verify(
            service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenPasswordIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        RecoverAccountModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "ABCDEF",
            NewPassword = null,
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey($"{nameof(RecoverAccountModel.Input)}.{nameof(RecoverAccountModel.RecoverAccountInputModel.NewPassword)}");
        serviceMock.Verify(
            service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenPasswordIsTooShort_ShouldReturnPageWithModelError()
    {
        // Arrange
        RecoverAccountModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "ABCDEF",
            NewPassword = "short",
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey($"{nameof(RecoverAccountModel.Input)}.{nameof(RecoverAccountModel.RecoverAccountInputModel.NewPassword)}");
        serviceMock.Verify(
            service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenBothFieldsMissing_ShouldNotCallService()
    {
        // Arrange
        RecoverAccountModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel();

        // Act
        await model.OnPostAsync();

        // Assert
        serviceMock.Verify(
            service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenInputIsValid_ShouldCallServiceWithMappedRequest()
    {
        // Arrange
        RecoverAccountRequest? capturedRequest = null;
        RecoverAccountModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "ABCDEF",
            NewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()))
            .Callback<RecoverAccountRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Success());

        // Act
        await model.OnPostAsync();

        // Assert
        serviceMock.Verify(
            service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Code.Should().Be("ABCDEF");
        capturedRequest.NewPassword.Should().Be("NewStrongPassword!1");
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceSucceeds_ShouldSetIsSuccess()
    {
        // Arrange
        RecoverAccountModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "ABCDEF",
            NewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await model.OnPostAsync();

        // Assert
        model.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_ShouldReturnPageWithModelError()
    {
        // Arrange
        RecoverAccountModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "XYZABC",
            NewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.invalid_recovery_code", "The recovery code is invalid.")));

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.IsSuccess.Should().BeFalse();
        model.ModelState.Should().ContainKey(string.Empty);
        model.ModelState[string.Empty]!.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_AndErrorCodeFoundInUsersResource_ShouldUseUsersResourceMessage()
    {
        // Arrange
        ConfigurableLocalizer<UsersResource> usersLoc = new();
        usersLoc.Set("users.invalid_recovery_code", "Recovery code not valid");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.Set("users.invalid_recovery_code", "Auth resource message");

        RecoverAccountModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "XYZABC",
            NewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.invalid_recovery_code", "The recovery code is invalid.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Recovery code not valid");
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_AndErrorCodeNotInUsersResource_ShouldUseAuthResourceMessage()
    {
        // Arrange
        ConfigurableLocalizer<UsersResource> usersLoc = new();
        usersLoc.SetNotFound("users.invalid_recovery_code");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.Set("users.invalid_recovery_code", "Auth fallback message");

        RecoverAccountModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "XYZABC",
            NewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.invalid_recovery_code", "The recovery code is invalid.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Auth fallback message");
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_AndErrorCodeInNeitherResource_ShouldUseFallbackMessage()
    {
        // Arrange
        ConfigurableLocalizer<UsersResource> usersLoc = new();
        usersLoc.SetNotFound("users.unknown_error");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.SetNotFound("users.unknown_error");
        authLoc.Set("Auth.RecoverAccount.Error.RecoveryFailed", "Recovery failed. Please try again.");

        RecoverAccountModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = new RecoverAccountModel.RecoverAccountInputModel
        {
            Code = "XYZABC",
            NewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.RecoverAccountAsync(It.IsAny<RecoverAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.unknown_error", "Unknown error.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Recovery failed. Please try again.");
    }

    private static RecoverAccountModel CreateModel(
        out Mock<IUserManagementService> serviceMock,
        IStringLocalizer<AuthResource>? authLocalizer = null,
        IStringLocalizer<UsersResource>? usersLocalizer = null)
    {
        serviceMock = new Mock<IUserManagementService>();

        RecoverAccountModel model = new(
            serviceMock.Object,
            authLocalizer ?? new TestAuthLocalizer(),
            usersLocalizer ?? new TestUsersLocalizer())
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

    private sealed class ConfigurableLocalizer<T> : IStringLocalizer<T>
    {
        private readonly Dictionary<string, LocalizedString> _overrides = [];

        public void Set(string key, string value)
        {
            _overrides[key] = new LocalizedString(key, value);
        }

        public void SetNotFound(string key)
        {
            _overrides[key] = new LocalizedString(key, key, resourceNotFound: true);
        }

        public LocalizedString this[string name]
            => _overrides.TryGetValue(name, out LocalizedString? entry) ? entry! : new(name, name);

        public LocalizedString this[string name, params object[] arguments]
            => _overrides.TryGetValue(name, out LocalizedString? entry)
                ? entry!
                : new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class TestAuthLocalizer : IStringLocalizer<AuthResource>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class TestUsersLocalizer : IStringLocalizer<UsersResource>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
