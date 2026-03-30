// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users.Entities;
using Beagl.WebApp.Pages.Account;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Beagl.WebApp.Tests.Pages.Account;

public sealed class LogoutModelTests
{
    [Fact]
    public async Task OnPostAsync_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock();
        signInManagerMock
            .Setup(manager => manager.SignOutAsync())
            .Returns(Task.CompletedTask);

        LogoutModel model = new(signInManagerMock.Object);

        // Act
        IActionResult result = await model.OnPostAsync();

        // Assert
        LocalRedirectResult redirectResult = result.Should().BeOfType<LocalRedirectResult>().Subject;
        redirectResult.Url.Should().Be("/account/login");
        signInManagerMock.Verify(manager => manager.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenSignOutFails_ShouldBubbleException()
    {
        // Arrange
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock();
        signInManagerMock
            .Setup(manager => manager.SignOutAsync())
            .ThrowsAsync(new InvalidOperationException("signout failed"));

        LogoutModel model = new(signInManagerMock.Object);

        // Act
        Func<Task> act = () => model.OnPostAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock()
    {
        Mock<IUserStore<ApplicationUser>> userStoreMock = new();
        Mock<UserManager<ApplicationUser>> userManagerMock = new(
            userStoreMock.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        Mock<IUserClaimsPrincipalFactory<ApplicationUser>> claimsFactoryMock = new();
        HttpContextAccessor httpContextAccessor = new();
        IOptions<IdentityOptions> identityOptions = Options.Create(new IdentityOptions());
        AuthenticationSchemeProvider schemeProvider = new(Options.Create(new AuthenticationOptions()));
        Mock<IUserConfirmation<ApplicationUser>> userConfirmationMock = new();

        return new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            httpContextAccessor,
            claimsFactoryMock.Object,
            identityOptions,
            NullLogger<SignInManager<ApplicationUser>>.Instance,
            schemeProvider,
            userConfirmationMock.Object);
    }
}
