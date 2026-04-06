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
using System.Security.Claims;

namespace Beagl.WebApp.Tests.Pages.Account;

public sealed class ChangePasswordModelTests
{
    [Fact]
    public void OnGet_ShouldLeaveModelStateValid()
    {
        ChangePasswordModel model = CreateModel(out _);

        model.OnGet();

        model.ModelState.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_WhenCurrentPasswordIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = null,
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(
            $"{nameof(ChangePasswordModel.Input)}.{nameof(ChangePasswordModel.ChangePasswordInputModel.CurrentPassword)}");
        serviceMock.Verify(
            service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenNewPasswordIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "OldPassword!1",
            NewPassword = null,
            ConfirmNewPassword = null,
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(
            $"{nameof(ChangePasswordModel.Input)}.{nameof(ChangePasswordModel.ChangePasswordInputModel.NewPassword)}");
        serviceMock.Verify(
            service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenNewPasswordIsTooShort_ShouldReturnPageWithModelError()
    {
        // Arrange
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "OldPassword!1",
            NewPassword = "short",
            ConfirmNewPassword = "short",
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(
            $"{nameof(ChangePasswordModel.Input)}.{nameof(ChangePasswordModel.ChangePasswordInputModel.NewPassword)}");
        serviceMock.Verify(
            service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenPasswordsDoNotMatch_ShouldReturnPageWithModelError()
    {
        // Arrange
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "OldPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "DifferentPassword!1",
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(
            $"{nameof(ChangePasswordModel.Input)}.{nameof(ChangePasswordModel.ChangePasswordInputModel.ConfirmNewPassword)}");
        serviceMock.Verify(
            service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenAllFieldsMissing_ShouldNotCallService()
    {
        // Arrange
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel();

        // Act
        await model.OnPostAsync();

        // Assert
        serviceMock.Verify(
            service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserIdIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock, userId: null);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "OldPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(string.Empty);
        serviceMock.Verify(
            service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenInputIsValid_ShouldCallServiceWithMappedRequest()
    {
        // Arrange
        ChangePasswordRequest? capturedRequest = null;
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock, userId: "user-42");
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "OldPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ChangePasswordRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Success());

        // Act
        await model.OnPostAsync();

        // Assert
        serviceMock.Verify(
            service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.UserId.Should().Be("user-42");
        capturedRequest.CurrentPassword.Should().Be("OldPassword!1");
        capturedRequest.NewPassword.Should().Be("NewStrongPassword!1");
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceSucceeds_ShouldSetIsSuccess()
    {
        // Arrange
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock, userId: "user-42");
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "OldPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
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
        ChangePasswordModel model = CreateModel(out Mock<IUserManagementService> serviceMock, userId: "user-42");
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "WrongPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.identity_error", "The user operation could not be completed.")));

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
        usersLoc.Set("users.identity_error", "Users resource message");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.Set("users.identity_error", "Auth resource message");

        ChangePasswordModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            userId: "user-42",
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "WrongPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.identity_error", "The user operation could not be completed.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Users resource message");
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_AndErrorCodeNotInUsersResource_ShouldUseAuthResourceMessage()
    {
        // Arrange
        ConfigurableLocalizer<UsersResource> usersLoc = new();
        usersLoc.SetNotFound("users.identity_error");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.Set("users.identity_error", "Auth fallback message");

        ChangePasswordModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            userId: "user-42",
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "WrongPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.identity_error", "The user operation could not be completed.")));

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
        authLoc.Set("Auth.ChangePassword.Error.ChangeFailed", "Password change failed. Please try again.");

        ChangePasswordModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            userId: "user-42",
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = new ChangePasswordModel.ChangePasswordInputModel
        {
            CurrentPassword = "WrongPassword!1",
            NewPassword = "NewStrongPassword!1",
            ConfirmNewPassword = "NewStrongPassword!1",
        };
        serviceMock
            .Setup(service => service.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.unknown_error", "Unknown error.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Password change failed. Please try again.");
    }

    private static ChangePasswordModel CreateModel(
        out Mock<IUserManagementService> serviceMock,
        string? userId = "user-1",
        IStringLocalizer<AuthResource>? authLocalizer = null,
        IStringLocalizer<UsersResource>? usersLocalizer = null)
    {
        serviceMock = new Mock<IUserManagementService>();

        DefaultHttpContext httpContext = new();
        if (userId is not null)
        {
            Claim[] claims = [new Claim(ClaimTypes.NameIdentifier, userId)];
            ClaimsIdentity identity = new(claims, "TestAuth");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        ChangePasswordModel model = new(
            serviceMock.Object,
            authLocalizer ?? new TestAuthLocalizer(),
            usersLocalizer ?? new TestUsersLocalizer())
        {
            PageContext = new PageContext(
                new ActionContext(
                    httpContext,
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
