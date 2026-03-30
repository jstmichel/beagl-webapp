// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
using Beagl.WebApp.Pages.Account;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Beagl.WebApp.Tests.Pages.Account;

public sealed class LogoutModelTests
{
    [Fact]
    public async Task OnPostAsync_ShouldSignOutAndRedirectToLogin()
    {
        Mock<ISharedLoginService> loginServiceMock = new();
        loginServiceMock
            .Setup(service => service.SignOutAsync())
            .Returns(Task.CompletedTask);

        LogoutModel model = new(loginServiceMock.Object);

        IActionResult result = await model.OnPostAsync();

        LocalRedirectResult redirectResult = result.Should().BeOfType<LocalRedirectResult>().Subject;
        redirectResult.Url.Should().Be("/account/login");
        loginServiceMock.Verify(service => service.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenSignOutFails_ShouldBubbleException()
    {
        Mock<ISharedLoginService> loginServiceMock = new();
        loginServiceMock
            .Setup(service => service.SignOutAsync())
            .ThrowsAsync(new InvalidOperationException("signout failed"));

        LogoutModel model = new(loginServiceMock.Object);

        Func<Task> act = () => model.OnPostAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
