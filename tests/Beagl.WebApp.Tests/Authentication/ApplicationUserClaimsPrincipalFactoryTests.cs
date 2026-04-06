// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Security.Claims;
using Beagl.Infrastructure.Users;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Beagl.WebApp.Tests.Authentication;

public sealed class ApplicationUserClaimsPrincipalFactoryTests
{
    [Fact]
    public async Task CreateAsync_WhenMustChangePasswordIsTrue_ShouldAddClaim()
    {
        // Arrange
        ApplicationUser user = new()
        {
            Id = "user-1",
            UserName = "testuser",
            MustChangePassword = true,
        };
        ApplicationUserClaimsPrincipalFactory factory = CreateFactory();

        // Act
        ClaimsPrincipal principal = await factory.CreateAsync(user);

        // Assert
        principal.HasClaim(
            ApplicationUserClaimsPrincipalFactory.MustChangePasswordClaimType, "true")
            .Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WhenMustChangePasswordIsFalse_ShouldNotAddClaim()
    {
        // Arrange
        ApplicationUser user = new()
        {
            Id = "user-2",
            UserName = "testuser2",
            MustChangePassword = false,
        };
        ApplicationUserClaimsPrincipalFactory factory = CreateFactory();

        // Act
        ClaimsPrincipal principal = await factory.CreateAsync(user);

        // Assert
        principal.HasClaim(
            ApplicationUserClaimsPrincipalFactory.MustChangePasswordClaimType, "true")
            .Should().BeFalse();
    }

    private static ApplicationUserClaimsPrincipalFactory CreateFactory()
    {
        Mock<IUserStore<ApplicationUser>> userStoreMock = new();
        IOptions<IdentityOptions> identityOptions = Options.Create(new IdentityOptions());

        Mock<UserManager<ApplicationUser>> userManagerMock = new(
            userStoreMock.Object,
            identityOptions,
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        Mock<IRoleStore<ApplicationRole>> roleStoreMock = new();

        Mock<RoleManager<ApplicationRole>> roleManagerMock = new(
            roleStoreMock.Object,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<ApplicationRole>>.Instance);

        userManagerMock
            .Setup(manager => manager.GetUserIdAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((ApplicationUser u) => u.Id);

        userManagerMock
            .Setup(manager => manager.GetUserNameAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((ApplicationUser u) => u.UserName);

        userManagerMock
            .Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

        userManagerMock
            .Setup(manager => manager.GetClaimsAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<Claim>());

        roleManagerMock
            .Setup(manager => manager.GetRoleNameAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync((ApplicationRole r) => r.Name);

        roleManagerMock
            .Setup(manager => manager.GetClaimsAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(new List<Claim>());

        return new ApplicationUserClaimsPrincipalFactory(
            userManagerMock.Object,
            roleManagerMock.Object,
            identityOptions);
    }
}
