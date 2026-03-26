// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
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

public sealed class RegisterModelTests
{
    [Fact]
    public void OnGet_ShouldLeaveModelStateValid()
    {
        RegisterModel model = CreateModel(out _);

        model.OnGet();

        model.ModelState.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_WhenFirstNameIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        RegisterModel.RegisterInputModel input = ValidInput();
        input.FirstName = "   ";
        model.Input = input;

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(RegisterModel.RegisterInputModel.FirstName));
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenLastNameIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        RegisterModel.RegisterInputModel input = ValidInput();
        input.LastName = null;
        model.Input = input;

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(RegisterModel.RegisterInputModel.LastName));
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserNameIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        RegisterModel.RegisterInputModel input = ValidInput();
        input.UserName = string.Empty;
        model.Input = input;

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(RegisterModel.RegisterInputModel.UserName));
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenPhoneNumberIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        RegisterModel.RegisterInputModel input = ValidInput();
        input.PhoneNumber = "   ";
        model.Input = input;

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(RegisterModel.RegisterInputModel.PhoneNumber));
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenEmailIsInvalidFormat_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        RegisterModel.RegisterInputModel input = ValidInput();
        input.Email = "not-an-email";
        model.Input = input;

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(RegisterModel.RegisterInputModel.Email));
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenEmailIsEmpty_ShouldNotAddEmailValidationError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = ValidInput(email: null);
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(StubUserDetails()));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState.Should().NotContainKey(nameof(RegisterModel.RegisterInputModel.Email));
    }

    [Fact]
    public async Task OnPostAsync_WhenPasswordIsMissing_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        RegisterModel.RegisterInputModel input = ValidInput();
        input.Password = null;
        model.Input = input;

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(RegisterModel.RegisterInputModel.Password));
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenPasswordIsTooShort_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        RegisterModel.RegisterInputModel input = ValidInput();
        input.Password = "short";
        model.Input = input;

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(RegisterModel.RegisterInputModel.Password));
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenMultipleFieldsAreMissing_ShouldNotCallService()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = new RegisterModel.RegisterInputModel();

        // Act
        await model.OnPostAsync();

        // Assert
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenInputIsValid_ShouldCallServiceWithMappedRequest()
    {
        // Arrange
        RegisterCitizenRequest? capturedRequest = null;
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = ValidInput();
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .Callback<RegisterCitizenRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Success(StubUserDetails()));

        // Act
        await model.OnPostAsync();

        // Assert
        serviceMock.Verify(
            s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.FirstName.Should().Be("John");
        capturedRequest.LastName.Should().Be("Smith");
        capturedRequest.UserName.Should().Be("jsmith");
        capturedRequest.PhoneNumber.Should().Be("555-0100");
        capturedRequest.Email.Should().BeNull();
        capturedRequest.Password.Should().Be("Password123!");
    }

    [Fact]
    public async Task OnPostAsync_WhenInputIsValid_ShouldRedirectToLoginOnSuccess()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = ValidInput();
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(StubUserDetails()));

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        LocalRedirectResult redirect = result.Should().BeOfType<LocalRedirectResult>().Subject;
        redirect.Url.Should().Be("/account/login");
    }

    [Fact]
    public async Task OnPostAsync_WhenEmailIsProvided_ShouldPassEmailToService()
    {
        // Arrange
        RegisterCitizenRequest? capturedRequest = null;
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = ValidInput(email: "john@example.com");
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .Callback<RegisterCitizenRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Success(StubUserDetails()));

        // Act
        await model.OnPostAsync();

        // Assert
        capturedRequest!.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task OnPostAsync_WhenEmailIsWhitespace_ShouldPassNullEmailToService()
    {
        // Arrange
        RegisterCitizenRequest? capturedRequest = null;
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = ValidInput(email: "   ");
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .Callback<RegisterCitizenRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Success(StubUserDetails()));

        // Act
        await model.OnPostAsync();

        // Assert
        capturedRequest!.Email.Should().BeNull();
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_ShouldReturnPageWithModelError()
    {
        // Arrange
        RegisterModel model = CreateModel(out Mock<IUserManagementService> serviceMock);
        model.Input = ValidInput();
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserDetailsDto>(new ResultError("users.duplicate_user_name", "The user name is already in use.")));

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(string.Empty);
        model.ModelState[string.Empty]!.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_AndErrorCodeFoundInUsersResource_ShouldUseUsersResourceMessage()
    {
        // Arrange
        ConfigurableLocalizer<UsersResource> usersLoc = new();
        usersLoc.Set("users.duplicate_user_name", "Username is already taken");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.Set("users.duplicate_user_name", "Auth resource message");

        RegisterModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = ValidInput();
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserDetailsDto>(new ResultError("users.duplicate_user_name", "The user name is already in use.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(e => e.ErrorMessage == "Username is already taken");
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_AndErrorCodeNotInUsersResource_ShouldUseAuthResourceMessage()
    {
        // Arrange
        ConfigurableLocalizer<UsersResource> usersLoc = new();
        usersLoc.SetNotFound("users.duplicate_user_name");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.Set("users.duplicate_user_name", "Auth fallback message");

        RegisterModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = ValidInput();
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserDetailsDto>(new ResultError("users.duplicate_user_name", "The user name is already in use.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(e => e.ErrorMessage == "Auth fallback message");
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceFails_AndErrorCodeInNeitherResource_ShouldUseFallbackMessage()
    {
        // Arrange
        ConfigurableLocalizer<UsersResource> usersLoc = new();
        usersLoc.SetNotFound("users.unknown_error");

        ConfigurableLocalizer<AuthResource> authLoc = new();
        authLoc.SetNotFound("users.unknown_error");
        authLoc.Set("Auth.Register.Error.RegistrationFailed", "Registration failed. Please try again.");

        RegisterModel model = CreateModel(
            out Mock<IUserManagementService> serviceMock,
            authLocalizer: authLoc,
            usersLocalizer: usersLoc);
        model.Input = ValidInput();
        serviceMock
            .Setup(s => s.RegisterCitizenAsync(It.IsAny<RegisterCitizenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserDetailsDto>(new ResultError("users.unknown_error", "Unknown error.")));

        // Act
        await model.OnPostAsync();

        // Assert
        model.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(e => e.ErrorMessage == "Registration failed. Please try again.");
    }

    private static RegisterModel CreateModel(
        out Mock<IUserManagementService> serviceMock,
        IStringLocalizer<AuthResource>? authLocalizer = null,
        IStringLocalizer<UsersResource>? usersLocalizer = null)
    {
        serviceMock = new Mock<IUserManagementService>();

        RegisterModel model = new(
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

    private static RegisterModel.RegisterInputModel ValidInput(string? email = null)
    {
        return new RegisterModel.RegisterInputModel
        {
            FirstName = "John",
            LastName = "Smith",
            UserName = "jsmith",
            PhoneNumber = "555-0100",
            Email = email,
            Password = "Password123!",
        };
    }

    private static UserDetailsDto StubUserDetails()
    {
        return new UserDetailsDto("id-1", "jsmith", "", null, false, false, UserRole.Citizen);
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
