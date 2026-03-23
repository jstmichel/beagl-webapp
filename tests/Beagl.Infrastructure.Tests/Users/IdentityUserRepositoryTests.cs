// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
using Beagl.Infrastructure.Users;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Beagl.Infrastructure.Tests.Users;

public class IdentityUserRepositoryTests
{
    [Theory]
    [InlineData("DuplicateUserName", "duplicate user", "users.duplicate_user_name", "The user name is already in use.")]
    [InlineData("DuplicateEmail", "duplicate email", "users.duplicate_email", "The email address is already in use.")]
    [InlineData("InvalidEmail", "invalid email", "users.invalid_email", "The email address is not valid.")]
    [InlineData("PasswordTooShort", "Password must be at least 8 characters.", "users.password_too_short", "Password must be at least 8 characters.")]
    [InlineData("SomeUnknownCode", "identity operation failed", "users.identity_error", "identity operation failed")]
    public async Task CreateAsync_WhenCreateFails_ShouldMapIdentityError(
        string identityCode,
        string identityDescription,
        string expectedCode,
        string expectedMessage)
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = identityCode, Description = identityDescription }));

        IdentityUserRepository repository = new(userManagerMock.Object);
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(expectedCode);
        result.Error.Message.Should().Be(expectedMessage);
        userManagerMock.Verify(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCreateFailsWithoutIdentityErrors_ShouldReturnGenericIdentityError()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        IdentityUserRepository repository = new(userManagerMock.Object);
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.identity_error");
        result.Error.Message.Should().Be("The user operation could not be completed.");
    }

    [Fact]
    public async Task CreateAsync_WhenAddToRoleFails_ShouldMapErrorAndRollbackCreatedUser()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleNotFound", Description = "role missing" }));
        userManagerMock
            .Setup(manager => manager.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object);
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.role_not_found");
        userManagerMock.Verify(manager => manager.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenRemoveFromRolesFails_ShouldReturnMappedError()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", PhoneNumber = "555-0100" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync(["Citizen"]);
        userManagerMock
            .Setup(manager => manager.RemoveFromRolesAsync(identityUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidEmail", Description = "bad email" }));

        IdentityUserRepository repository = new(userManagerMock.Object);
        UpdateUserAccount request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_email");
        userManagerMock.Verify(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenAddToRoleFailsAfterRemove_ShouldReturnMappedError()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", PhoneNumber = "555-0100" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync(["Citizen"]);
        userManagerMock
            .Setup(manager => manager.RemoveFromRolesAsync(identityUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(identityUser, UserRole.Employee.ToString()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleNotFound", Description = "role missing" }));

        IdentityUserRepository repository = new(userManagerMock.Object);
        UpdateUserAccount request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.role_not_found");
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleReplacementSucceeds_ShouldReturnUpdatedMappedUser()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", PhoneNumber = "555-0100" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .SetupSequence(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync(["Citizen"])
            .ReturnsAsync([UserRole.Administrator.ToString()]);
        userManagerMock
            .Setup(manager => manager.RemoveFromRolesAsync(identityUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(identityUser, UserRole.Administrator.ToString()))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object);
        UpdateUserAccount request = new("user-1", "alex-updated", "alex-updated@example.com", "555-9999", UserRole.Administrator);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-1");
        result.Value.UserName.Should().Be("alex-updated");
        result.Value.Email.Should().Be("alex-updated@example.com");
        result.Value.PhoneNumber.Should().Be("555-9999");
        result.Value.Role.Should().Be(UserRole.Administrator);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        Mock<IUserStore<ApplicationUser>> userStoreMock = new();

        return new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Logger<UserManager<ApplicationUser>>(new LoggerFactory()));
    }
}
