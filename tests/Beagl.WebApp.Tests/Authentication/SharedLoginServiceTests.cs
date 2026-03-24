// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users.Entities;
using Beagl.WebApp.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Beagl.WebApp.Tests.Authentication;

public sealed class SharedLoginServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ShouldUseUserNameLookupFirst_WhenIdentifierContainsAtSymbol()
    {
        ApplicationUser user = new() { UserName = "employee1@local", Email = "employee1@beagl.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock.Setup(manager => manager.FindByNameAsync("employee1@local"))
            .ReturnsAsync(user);
        signInManagerMock.Setup(manager => manager.PasswordSignInAsync(user, "StrongPassword!1", false, false))
            .ReturnsAsync(SignInResult.Success);

        SharedLoginService service = new(userManagerMock.Object, signInManagerMock.Object);

        SharedLoginStatus result = await service.AuthenticateAsync("employee1@local", "StrongPassword!1", false);

        result.Should().Be(SharedLoginStatus.Succeeded);
        userManagerMock.Verify(manager => manager.FindByNameAsync("employee1@local"), Times.Once);
        userManagerMock.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldUseEmailLookup_WhenUserNameIsNotFound()
    {
        ApplicationUser user = new() { UserName = "employee1", Email = "employee1@beagl.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock.Setup(manager => manager.FindByNameAsync("employee1@beagl.local"))
            .ReturnsAsync((ApplicationUser?)null);
        userManagerMock.Setup(manager => manager.FindByEmailAsync("employee1@beagl.local"))
            .ReturnsAsync(user);
        signInManagerMock.Setup(manager => manager.PasswordSignInAsync(user, "StrongPassword!1", false, false))
            .ReturnsAsync(SignInResult.Success);

        SharedLoginService service = new(userManagerMock.Object, signInManagerMock.Object);

        SharedLoginStatus result = await service.AuthenticateAsync("employee1@beagl.local", "StrongPassword!1", false);

        result.Should().Be(SharedLoginStatus.Succeeded);
        userManagerMock.Verify(manager => manager.FindByNameAsync("employee1@beagl.local"), Times.Once);
        userManagerMock.Verify(manager => manager.FindByEmailAsync("employee1@beagl.local"), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldUseUserNameLookup_WhenIdentifierIsNotEmail()
    {
        ApplicationUser user = new() { UserName = "citizen1", Email = "citizen1@beagl.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock.Setup(manager => manager.FindByNameAsync("citizen1"))
            .ReturnsAsync(user);
        signInManagerMock.Setup(manager => manager.PasswordSignInAsync(user, "StrongPassword!1", true, false))
            .ReturnsAsync(SignInResult.Success);

        SharedLoginService service = new(userManagerMock.Object, signInManagerMock.Object);

        SharedLoginStatus result = await service.AuthenticateAsync("citizen1", "StrongPassword!1", true);

        result.Should().Be(SharedLoginStatus.Succeeded);
        userManagerMock.Verify(manager => manager.FindByNameAsync("citizen1"), Times.Once);
        userManagerMock.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnInvalidCredentials_WhenNoUserMatchesIdentifier()
    {
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock.Setup(manager => manager.FindByNameAsync("+15550001111"))
            .ReturnsAsync((ApplicationUser?)null);

        SharedLoginService service = new(userManagerMock.Object, signInManagerMock.Object);

        SharedLoginStatus result = await service.AuthenticateAsync("+15550001111", "StrongPassword!1", false);

        result.Should().Be(SharedLoginStatus.InvalidCredentials);
        userManagerMock.Verify(manager => manager.FindByNameAsync("+15550001111"), Times.Once);
        userManagerMock.Verify(manager => manager.FindByEmailAsync("+15550001111"), Times.Once);
        signInManagerMock.Verify(
            manager => manager.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldUseUserNameLookup_WhenIdentifierLooksLikePhoneNumber()
    {
        ApplicationUser user = new() { UserName = "+1 (555) 000-1111", Email = "citizen-phone@beagl.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock.Setup(manager => manager.FindByNameAsync("+1 (555) 000-1111"))
            .ReturnsAsync(user);
        signInManagerMock.Setup(manager => manager.PasswordSignInAsync(user, "StrongPassword!1", false, false))
            .ReturnsAsync(SignInResult.Success);

        SharedLoginService service = new(userManagerMock.Object, signInManagerMock.Object);

        SharedLoginStatus result = await service.AuthenticateAsync("+1 (555) 000-1111", "StrongPassword!1", false);

        result.Should().Be(SharedLoginStatus.Succeeded);
        userManagerMock.Verify(manager => manager.FindByNameAsync("+1 (555) 000-1111"), Times.Once);
        userManagerMock.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnLockedOut_WhenIdentityReturnsLockedOut()
    {
        ApplicationUser user = new() { UserName = "employee2", Email = "employee2@beagl.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock.Setup(manager => manager.FindByNameAsync("employee2"))
            .ReturnsAsync(user);
        signInManagerMock.Setup(manager => manager.PasswordSignInAsync(user, "StrongPassword!1", false, false))
            .ReturnsAsync(SignInResult.LockedOut);

        SharedLoginService service = new(userManagerMock.Object, signInManagerMock.Object);

        SharedLoginStatus result = await service.AuthenticateAsync("employee2", "StrongPassword!1", false);

        result.Should().Be(SharedLoginStatus.LockedOut);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNotAllowed_WhenIdentityReturnsNotAllowed()
    {
        ApplicationUser user = new() { UserName = "employee3", Email = "employee3@beagl.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        Mock<SignInManager<ApplicationUser>> signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock.Setup(manager => manager.FindByNameAsync("employee3"))
            .ReturnsAsync(user);
        signInManagerMock.Setup(manager => manager.PasswordSignInAsync(user, "StrongPassword!1", false, false))
            .ReturnsAsync(SignInResult.NotAllowed);

        SharedLoginService service = new(userManagerMock.Object, signInManagerMock.Object);

        SharedLoginStatus result = await service.AuthenticateAsync("employee3", "StrongPassword!1", false);

        result.Should().Be(SharedLoginStatus.NotAllowed);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        Mock<IUserStore<ApplicationUser>> storeMock = new();
        IOptions<IdentityOptions> identityOptions = Options.Create(new IdentityOptions());
        PasswordHasher<ApplicationUser> passwordHasher = new();
        List<IUserValidator<ApplicationUser>> userValidators = [new UserValidator<ApplicationUser>()];
        List<IPasswordValidator<ApplicationUser>> passwordValidators = [new PasswordValidator<ApplicationUser>()];
        UpperInvariantLookupNormalizer normalizer = new();
        IdentityErrorDescriber errorDescriber = new();
        ServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();
        NullLogger<UserManager<ApplicationUser>> logger = NullLogger<UserManager<ApplicationUser>>.Instance;

        Mock<UserManager<ApplicationUser>> userManagerMock = new(
            storeMock.Object,
            identityOptions,
            passwordHasher,
            userValidators,
            passwordValidators,
            normalizer,
            errorDescriber,
            serviceProvider,
            logger);

        return userManagerMock;
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
    {
        Mock<IUserClaimsPrincipalFactory<ApplicationUser>> claimsFactoryMock = new();
        HttpContextAccessor httpContextAccessor = new();
        IOptions<IdentityOptions> identityOptions = Options.Create(new IdentityOptions());
        NullLogger<SignInManager<ApplicationUser>> logger = NullLogger<SignInManager<ApplicationUser>>.Instance;
        AuthenticationSchemeProvider schemeProvider = new(Options.Create(new AuthenticationOptions()));
        Mock<IUserConfirmation<ApplicationUser>> userConfirmationMock = new();

        Mock<SignInManager<ApplicationUser>> signInManagerMock = new(
            userManager,
            httpContextAccessor,
            claimsFactoryMock.Object,
            identityOptions,
            logger,
            schemeProvider,
            userConfirmationMock.Object);

        return signInManagerMock;
    }
}
