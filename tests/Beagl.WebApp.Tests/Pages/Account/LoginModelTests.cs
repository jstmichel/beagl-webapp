// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
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
using System.Globalization;
using Moq;

namespace Beagl.WebApp.Tests.Pages.Account;

public sealed class LoginModelTests
{
    [Fact]
    public void OnGet_ShouldSetReturnUrl()
    {
        LoginModel model = CreateModel(out _, out _);

        model.OnGet("/employee/users");

        model.ReturnUrl.Should().Be("/employee/users");
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenIdentifierIsMissing()
    {
        LoginModel model = CreateModel(out TestSharedLoginService service, out _);
        model.Input = new LoginModel.LoginInputModel
        {
            Identifier = string.Empty,
            Password = "StrongPassword!1",
            RememberMe = false,
        };

        IActionResult result = await model.OnPostAsync("/employee/users");

        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(LoginModel.LoginInputModel.Identifier));
        service.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenPasswordIsMissing()
    {
        LoginModel model = CreateModel(out TestSharedLoginService service, out _);
        model.Input = new LoginModel.LoginInputModel
        {
            Identifier = "employee1",
            Password = string.Empty,
            RememberMe = false,
        };

        IActionResult result = await model.OnPostAsync("/employee/users");

        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(nameof(LoginModel.LoginInputModel.Password));
        service.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToLocalReturnUrl_WhenAuthenticationSucceeds()
    {
        LoginModel model = CreateModel(out TestSharedLoginService service, out Mock<IUrlHelper> urlHelperMock);
        model.Input = new LoginModel.LoginInputModel
        {
            Identifier = "employee1",
            Password = "StrongPassword!1",
            RememberMe = true,
        };

        service.NextStatus = SharedLoginStatus.Succeeded;
        urlHelperMock
            .Setup(urlHelper => urlHelper.IsLocalUrl("/employee/users"))
            .Returns(true);

        IActionResult result = await model.OnPostAsync("/employee/users");

        LocalRedirectResult redirectResult = result.Should().BeOfType<LocalRedirectResult>().Subject;
        redirectResult.Url.Should().Be("/employee/users");
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToRoot_WhenReturnUrlIsNotLocal()
    {
        LoginModel model = CreateModel(out TestSharedLoginService service, out Mock<IUrlHelper> urlHelperMock);
        model.Input = new LoginModel.LoginInputModel
        {
            Identifier = "employee1",
            Password = "StrongPassword!1",
            RememberMe = false,
        };

        service.NextStatus = SharedLoginStatus.Succeeded;
        urlHelperMock
            .Setup(urlHelper => urlHelper.IsLocalUrl("https://example.com/redirect"))
            .Returns(false);

        IActionResult result = await model.OnPostAsync("https://example.com/redirect");

        LocalRedirectResult redirectResult = result.Should().BeOfType<LocalRedirectResult>().Subject;
        redirectResult.Url.Should().Be("/");
    }

    [Theory]
    [InlineData((int)SharedLoginStatus.LockedOut, "Auth.Login.Error.LockedOut")]
    [InlineData((int)SharedLoginStatus.NotAllowed, "Auth.Login.Error.NotAllowed")]
    [InlineData((int)SharedLoginStatus.InvalidCredentials, "Auth.Login.Error.InvalidCredentials")]
    public async Task OnPostAsync_ShouldReturnPageWithErrorMessage_WhenAuthenticationFails(int statusValue, string expectedErrorKey)
    {
        SharedLoginStatus status = (SharedLoginStatus)statusValue;
        LoginModel model = CreateModel(out TestSharedLoginService service, out _);
        model.Input = new LoginModel.LoginInputModel
        {
            Identifier = "employee1",
            Password = "StrongPassword!1",
            RememberMe = false,
        };

        service.NextStatus = status;

        IActionResult result = await model.OnPostAsync("/employee/users");

        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(string.Empty);
        ModelStateEntry modelStateEntry = model.ModelState[string.Empty]!;
        modelStateEntry.Errors.Should().ContainSingle(error => error.ErrorMessage == expectedErrorKey);
    }

    private static LoginModel CreateModel(
        out TestSharedLoginService service,
        out Mock<IUrlHelper> urlHelperMock)
    {
        service = new TestSharedLoginService();
        TestLocalizer localizer = new();
        urlHelperMock = new Mock<IUrlHelper>();

        LoginModel model = new(service, localizer)
        {
            Url = urlHelperMock.Object,
            PageContext = new PageContext(
                new ActionContext(
                    new DefaultHttpContext(),
                    new RouteData(),
                    new ActionDescriptor(),
                    new ModelStateDictionary())),
        };

        return model;
    }

    private sealed class TestSharedLoginService : ISharedLoginService
    {
        public int CallCount { get; private set; }

        public SharedLoginStatus NextStatus { get; set; } = SharedLoginStatus.Succeeded;

        public Task<SharedLoginStatus> AuthenticateAsync(string identifier, string password, bool rememberMe)
        {
            CallCount++;
            return Task.FromResult(NextStatus);
        }

        public Task SignOutAsync()
        {
            return Task.CompletedTask;
        }
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
