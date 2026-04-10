// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
using Beagl.Infrastructure.Users;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Beagl.Infrastructure.Tests.Users;

public class IdentityUserRepositoryTests
{
    [Fact]
    public void Constructor_WithNullUserManager_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new IdentityUserRepository(null!, CreateDbContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullApplicationDbContext_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new IdentityUserRepository(CreateUserManagerMock().Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_WhenCreateAndRoleAssignmentSucceed_ShouldReturnMappedUser()
    {
        // Arrange
        ApplicationUser createdUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", PhoneNumber = "555-0100" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                createdUser = user;
                createdUser.Id = "user-1";
            })
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.Employee.ToString()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirmation-token");
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-1");
        result.Value.UserName.Should().Be("alex");
        result.Value.Email.Should().Be("alex@example.com");
        result.Value.PhoneNumber.Should().Be("555-0100");
        result.Value.EmailConfirmed.Should().BeFalse();
        result.Value.Role.Should().Be(UserRole.Employee);
        result.Value.EmailConfirmationToken.Should().Be("confirmation-token");
        userManagerMock.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmailConfirmedRequest_ShouldCreateConfirmedUser()
    {
        // Arrange
        ApplicationUser? capturedUser = null;

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                capturedUser = user;
                capturedUser.Id = "user-1";
            })
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.Administrator.ToString()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync([UserRole.Administrator.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        CreateUserAccount request = new(
            "admin",
            "admin@example.com",
            null,
            "Password123!",
            UserRole.Administrator,
            EmailConfirmed: true);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.EmailConfirmed.Should().BeTrue();
        result.Value!.EmailConfirmed.Should().BeTrue();
        result.Value.EmailConfirmationToken.Should().BeNull();
        userManagerMock.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

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

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(expectedCode);
        result.Error.Message.Should().Be(expectedMessage);
        userManagerMock.Verify(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCreateFailsWithoutIdentityErrors_ShouldReturnGenericIdentityError()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.identity_error");
        result.Error.Message.Should().Be("The user operation could not be completed.");
        userManagerMock.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
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

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.role_not_found");
        userManagerMock.Verify(manager => manager.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
        userManagerMock.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ShouldReturnDuplicateEmailError()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = "existing-user",
            UserName = "existing",
            NormalizedUserName = "EXISTING",
            Email = "alex@example.com",
            NormalizedEmail = "ALEX@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await context.SaveChangesAsync();

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        CreateUserAccount request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.duplicate_email");
        userManagerMock.Verify(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
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

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
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

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateUserAccount request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.role_not_found");
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailAlreadyUsedByAnotherUser_ShouldReturnDuplicateEmailError()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = "user-2",
            UserName = "bob",
            NormalizedUserName = "BOB",
            Email = "alex@example.com",
            NormalizedEmail = "ALEX@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await context.SaveChangesAsync();

        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "other@example.com",
            PhoneNumber = "555-0100",
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        UpdateUserAccount request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.duplicate_email");
        userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailBelongsToCurrentUser_ShouldAllowUpdate()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = "user-1",
            UserName = "alex",
            NormalizedUserName = "ALEX",
            Email = "alex@example.com",
            NormalizedEmail = "ALEX@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await context.SaveChangesAsync();

        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", PhoneNumber = "555-0100" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .SetupSequence(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Citizen.ToString()])
            .ReturnsAsync([UserRole.Employee.ToString()]);
        userManagerMock
            .Setup(manager => manager.RemoveFromRolesAsync(identityUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(identityUser, UserRole.Employee.ToString()))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        UpdateUserAccount request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Role.Should().Be(UserRole.Employee);
        userManagerMock.Verify(manager => manager.UpdateAsync(identityUser), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateUserAccount request = new("missing-id", "bob", "bob@example.com", "555-0200", UserRole.Citizen);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateFails_ShouldMapIdentityError()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "duplicate email" }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateUserAccount request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Citizen);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.duplicate_email");
        userManagerMock.Verify(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
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

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
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

    [Fact]
    public async Task DeleteAsync_WhenUserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.DeleteAsync("missing-id", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task DeleteAsync_WhenDeleteFails_ShouldMapIdentityError()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.DeleteAsync(identityUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "SomeError", Description = "delete failed" }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.DeleteAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.identity_error");
    }

    [Fact]
    public async Task DeleteAsync_WhenSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.DeleteAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.DeleteAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAccountAsync_WhenUserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.ConfirmAccountAsync("missing-id", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task ConfirmAccountAsync_WhenUserAlreadyConfirmed_ShouldSkipUpdate()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            PhoneNumber = "555-0100",
            EmailConfirmed = true,
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.ConfirmAccountAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EmailConfirmed.Should().BeTrue();
        userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAccountAsync_WhenUpdateFails_ShouldMapIdentityError()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            EmailConfirmed = false,
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidEmail", Description = "invalid email" }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.ConfirmAccountAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_email");
    }

    [Fact]
    public async Task GetPageAsync_ShouldMapRolesWithoutCallingUserManagerGetRolesAsync()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.Roles.AddRange(
            new ApplicationRole { Id = "role-admin", Name = UserRole.Administrator.ToString(), NormalizedName = UserRole.Administrator.ToString().ToUpperInvariant() },
            new ApplicationRole { Id = "role-employee", Name = UserRole.Employee.ToString(), NormalizedName = UserRole.Employee.ToString().ToUpperInvariant() });
        context.Users.AddRange(
            new ApplicationUser
            {
                Id = "user-1",
                UserName = "alex",
                NormalizedUserName = "ALEX",
                Email = "alex@example.com",
                NormalizedEmail = "ALEX@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            },
            new ApplicationUser
            {
                Id = "user-2",
                UserName = "bob",
                NormalizedUserName = "BOB",
                Email = "bob@example.com",
                NormalizedEmail = "BOB@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            });
        context.Set<IdentityUserRole<string>>().AddRange(
            new IdentityUserRole<string> { UserId = "user-1", RoleId = "role-admin" },
            new IdentityUserRole<string> { UserId = "user-2", RoleId = "role-employee" });
        await context.SaveChangesAsync();

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ThrowsAsync(new InvalidOperationException("GetRolesAsync should not be called for paged queries."));

        IdentityUserRepository repository = new(userManagerMock.Object, context);

        // Act
        UsersPage result = await repository.GetPageAsync(new GetUsersPageQuery(null, 1, 10), CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(2);
        result.Users[0].Role.Should().Be(UserRole.Administrator);
        result.Users[1].Role.Should().Be(UserRole.Employee);
        userManagerMock.Verify(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<string> result = await repository.GenerateEmailConfirmationTokenAsync("missing-id", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_WhenUserAlreadyConfirmed_ShouldReturnAlreadyConfirmedError()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", EmailConfirmed = true };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<string> result = await repository.GenerateEmailConfirmationTokenAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.already_confirmed");
        userManagerMock.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_WhenUserHasNoEmail_ShouldReturnEmailRequiredError()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = null, EmailConfirmed = false };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<string> result = await repository.GenerateEmailConfirmationTokenAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.email_required");
        userManagerMock.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_WhenSucceeds_ShouldReturnToken()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", EmailConfirmed = false };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GenerateEmailConfirmationTokenAsync(identityUser))
            .ReturnsAsync("confirmation-token-123");

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<string> result = await repository.GenerateEmailConfirmationTokenAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("confirmation-token-123");
    }

    [Fact]
    public async Task ConfirmAccountByTokenAsync_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.ConfirmAccountByTokenAsync("missing-id", "some-token", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task ConfirmAccountByTokenAsync_WhenTokenIsInvalid_ShouldReturnInvalidTokenError()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", EmailConfirmed = false };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.ConfirmEmailAsync(identityUser, "bad-token"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.ConfirmAccountByTokenAsync("user-1", "bad-token", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_token");
    }

    [Fact]
    public async Task ConfirmAccountByTokenAsync_WhenTokenIsValid_ShouldReturnSuccess()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", EmailConfirmed = false };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.ConfirmEmailAsync(identityUser, "valid-token"))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.ConfirmAccountByTokenAsync("user-1", "valid-token", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterCitizenAsync_WhenSucceeds_ShouldReturnMappedUserWithCitizenRole()
    {
        // Arrange
        ApplicationDbContext context = CreateSqliteContext();
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.Id = "citizen-1";
                user.SecurityStamp = Guid.NewGuid().ToString();
                context.Users.Add(user);
                context.SaveChanges();
            })
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.Citizen.ToString()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("citizen-token");
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync([UserRole.Citizen.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        RegisterCitizenAccount account = new("John", "Doe", "johndoe", "555-0100", "john@example.com", "Password123!");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.RegisterCitizenAsync(account, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("citizen-1");
        result.Value.UserName.Should().Be("johndoe");
        result.Value.Email.Should().Be("john@example.com");
        result.Value.Role.Should().Be(UserRole.Citizen);
        result.Value.EmailConfirmationToken.Should().Be("citizen-token");
        context.CitizenProfiles.Should().ContainSingle(profile =>
            profile.UserId == "citizen-1"
            && profile.FirstName == "John"
            && profile.LastName == "Doe");
    }

    [Fact]
    public async Task RegisterCitizenAsync_WithNullEmail_ShouldSkipEmailCheckAndSucceed()
    {
        // Arrange
        ApplicationDbContext context = CreateSqliteContext();
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.Id = "citizen-2";
                user.SecurityStamp = Guid.NewGuid().ToString();
                context.Users.Add(user);
                context.SaveChanges();
            })
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.Citizen.ToString()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync([UserRole.Citizen.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        RegisterCitizenAccount account = new("Jane", "Smith", "janesmith", "555-0200", null, "Password123!");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.RegisterCitizenAsync(account, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EmailConfirmationToken.Should().BeNull();
        userManagerMock.Verify(manager => manager.CreateAsync(
            It.Is<ApplicationUser>(user => user.Email == null),
            "Password123!"), Times.Once);
    }

    [Fact]
    public async Task RegisterCitizenAsync_WhenEmailAlreadyExists_ShouldReturnDuplicateEmailError()
    {
        // Arrange
        ApplicationDbContext context = CreateSqliteContext();
        context.Users.Add(new ApplicationUser
        {
            Id = "existing-user",
            UserName = "existing",
            NormalizedUserName = "EXISTING",
            Email = "john@example.com",
            NormalizedEmail = "JOHN@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await context.SaveChangesAsync();

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        RegisterCitizenAccount account = new("John", "Doe", "johndoe", "555-0100", "john@example.com", "Password123!");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.RegisterCitizenAsync(account, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.duplicate_email");
        userManagerMock.Verify(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCitizenAsync_WhenCreateFails_ShouldRollbackAndReturnError()
    {
        // Arrange
        ApplicationDbContext context = CreateSqliteContext();
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordTooShort", Description = "Password too short." }));

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        RegisterCitizenAccount account = new("John", "Doe", "johndoe", "555-0100", null, "short");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.RegisterCitizenAsync(account, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.password_too_short");
        userManagerMock.Verify(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        context.CitizenProfiles.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCitizenAsync_WhenAddToRoleFails_ShouldRollbackAndReturnError()
    {
        // Arrange
        ApplicationDbContext context = CreateSqliteContext();
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) => user.Id = "citizen-1")
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.Citizen.ToString()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleNotFound", Description = "role missing" }));

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        RegisterCitizenAccount account = new("John", "Doe", "johndoe", "555-0100", null, "Password123!");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.RegisterCitizenAsync(account, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.role_not_found");
        context.CitizenProfiles.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenUserHasNoExistingRoles_ShouldSkipRemoveFromRolesAndAddNewRole()
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
            .ReturnsAsync([])
            .ReturnsAsync([UserRole.Employee.ToString()]);
        userManagerMock
            .Setup(manager => manager.AddToRoleAsync(identityUser, UserRole.Employee.ToString()))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateUserAccount request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be(UserRole.Employee);
        userManagerMock.Verify(manager => manager.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        userManagerMock.Verify(manager => manager.AddToRoleAsync(identityUser, UserRole.Employee.ToString()), Times.Once);
    }

    [Fact]
    public async Task FindByIdentifierAsync_WithNullIdentifier_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Func<Task> act = async () => await repository.FindByIdentifierAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task FindByIdentifierAsync_WhenUserFoundByUserName_ShouldReturnUser()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", PhoneNumber = "555-0100", EmailConfirmed = true };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByNameAsync("alex"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        UserAccount? result = await repository.FindByIdentifierAsync("alex", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("user-1");
        result.UserName.Should().Be("alex");
        userManagerMock.Verify(manager => manager.FindByNameAsync("alex"), Times.Once);
        userManagerMock.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FindByIdentifierAsync_WhenUserNotFoundByUserName_ShouldFallBackToEmail()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", PhoneNumber = "555-0100", EmailConfirmed = true };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByNameAsync("alex@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        userManagerMock
            .Setup(manager => manager.FindByEmailAsync("alex@example.com"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        UserAccount? result = await repository.FindByIdentifierAsync("alex@example.com", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("user-1");
        userManagerMock.Verify(manager => manager.FindByNameAsync("alex@example.com"), Times.Once);
        userManagerMock.Verify(manager => manager.FindByEmailAsync("alex@example.com"), Times.Once);
    }

    [Fact]
    public async Task FindByIdentifierAsync_WhenNoUserFound_ShouldReturnNull()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByNameAsync("unknown"))
            .ReturnsAsync((ApplicationUser?)null);
        userManagerMock
            .Setup(manager => manager.FindByEmailAsync("unknown"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        UserAccount? result = await repository.FindByIdentifierAsync("unknown", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdentifierAsync_ShouldTrimIdentifier()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", EmailConfirmed = true };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByNameAsync("alex"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        UserAccount? result = await repository.FindByIdentifierAsync("  alex  ", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        userManagerMock.Verify(manager => manager.FindByNameAsync("alex"), Times.Once);
    }

    [Fact]
    public async Task FindByIdentifierAsync_ShouldMapRecoveryCode()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", Email = "alex@example.com", EmailConfirmed = true, RecoveryCode = "ABCDEF" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByNameAsync("alex"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        UserAccount? result = await repository.FindByIdentifierAsync("alex", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.RecoveryCode.Should().Be("ABCDEF");
    }

    [Fact]
    public async Task GenerateRecoveryCodeAsync_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.GenerateRecoveryCodeAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task GenerateRecoveryCodeAsync_WhenUserFound_ShouldSetRecoveryCodeAndSucceed()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.GenerateRecoveryCodeAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        identityUser.RecoveryCode.Should().NotBeNull();
        identityUser.RecoveryCode.Should().HaveLength(6);
        identityUser.RecoveryCode.Should().MatchRegex("^[A-Z]+$");
        userManagerMock.Verify(manager => manager.UpdateAsync(identityUser), Times.Once);
    }

    [Fact]
    public async Task GenerateRecoveryCodeAsync_WhenUpdateFails_ShouldReturnFailure()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ConcurrencyFailure", Description = "Concurrency failure." }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result result = await repository.GenerateRecoveryCodeAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordByRecoveryCodeAsync_WhenResetPasswordFails_ShouldReturnMappedError()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = "user-1",
            UserName = "alex",
            NormalizedUserName = "ALEX",
            Email = "alex@example.com",
            NormalizedEmail = "ALEX@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
            RecoveryCode = "ABCDEF",
        });
        await context.SaveChangesAsync();

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);
        userManagerMock
            .Setup(manager => manager.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("reset-token");
        userManagerMock
            .Setup(manager => manager.ResetPasswordAsync(It.IsAny<ApplicationUser>(), "reset-token", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordTooShort", Description = "Password too short." }));

        IdentityUserRepository repository = new(userManagerMock.Object, context);

        // Act
        Beagl.Domain.Results.Result result = await repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "short", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.password_too_short");
        userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordByRecoveryCodeAsync_WhenUpdateAfterResetFails_ShouldReturnMappedError()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = "user-1",
            UserName = "alex",
            NormalizedUserName = "ALEX",
            Email = "alex@example.com",
            NormalizedEmail = "ALEX@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
            RecoveryCode = "ABCDEF",
        });
        await context.SaveChangesAsync();

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);
        userManagerMock
            .Setup(manager => manager.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("reset-token");
        userManagerMock
            .Setup(manager => manager.ResetPasswordAsync(It.IsAny<ApplicationUser>(), "reset-token", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ConcurrencyFailure", Description = "Concurrency failure." }));

        IdentityUserRepository repository = new(userManagerMock.Object, context);

        // Act
        Beagl.Domain.Results.Result result = await repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd!1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.identity_error");
    }

    [Fact]
    public async Task UpdateCitizenIdentityAsync_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateCitizenIdentity identity = new("missing-id", "555-0100", "alex@example.com");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateCitizenIdentityAsync(identity, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task UpdateCitizenIdentityAsync_WhenEmailAlreadyUsedByAnotherUser_ShouldReturnDuplicateEmailError()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = "user-2",
            UserName = "bob",
            NormalizedUserName = "BOB",
            Email = "bob@example.com",
            NormalizedEmail = "BOB@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await context.SaveChangesAsync();

        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            PhoneNumber = "555-0100",
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(context.Users);
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);

        IdentityUserRepository repository = new(userManagerMock.Object, context);
        UpdateCitizenIdentity identity = new("user-1", "555-0100", "bob@example.com");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateCitizenIdentityAsync(identity, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.duplicate_email");
        userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCitizenIdentityAsync_WhenEmailChanges_ShouldResetEmailConfirmedAndUpdateNormalizedEmail()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            PhoneNumber = "555-0100",
            EmailConfirmed = true,
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Citizen.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateCitizenIdentity identity = new("user-1", "555-9999", "newemail@example.com");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateCitizenIdentityAsync(identity, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        identityUser.EmailConfirmed.Should().BeFalse();
        identityUser.NormalizedEmail.Should().Be("NEWEMAIL@EXAMPLE.COM");
        identityUser.Email.Should().Be("newemail@example.com");
        identityUser.PhoneNumber.Should().Be("555-9999");
        result.Value!.Role.Should().Be(UserRole.Citizen);
    }

    [Fact]
    public async Task UpdateCitizenIdentityAsync_WhenEmailUnchanged_ShouldPreserveEmailConfirmation()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            PhoneNumber = "555-0100",
            EmailConfirmed = true,
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Citizen.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateCitizenIdentity identity = new("user-1", "555-9999", "alex@example.com");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateCitizenIdentityAsync(identity, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        identityUser.EmailConfirmed.Should().BeTrue();
        identityUser.PhoneNumber.Should().Be("555-9999");
    }

    [Fact]
    public async Task UpdateCitizenIdentityAsync_WithNullEmail_ShouldSkipEmailCheckAndSucceed()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            PhoneNumber = "555-0100",
            EmailConfirmed = true,
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Citizen.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateCitizenIdentity identity = new("user-1", "555-9999", null);

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateCitizenIdentityAsync(identity, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        identityUser.Email.Should().BeNull();
        identityUser.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCitizenIdentityAsync_WhenUpdateFails_ShouldReturnMappedError()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            PhoneNumber = "555-0100",
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "duplicate email" }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());
        UpdateCitizenIdentity identity = new("user-1", "555-9999", "new@example.com");

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UpdateCitizenIdentityAsync(identity, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.duplicate_email");
    }

    [Fact]
    public async Task ClearRecoveryCodeAsync_WhenUserNotFound_ShouldReturnWithoutError()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Func<Task> act = async () => await repository.ClearRecoveryCodeAsync("user-1", CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ClearRecoveryCodeAsync_WhenUserHasNoRecoveryCode_ShouldReturnWithoutUpdate()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", RecoveryCode = null };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        await repository.ClearRecoveryCodeAsync("user-1", CancellationToken.None);

        // Assert
        userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ClearRecoveryCodeAsync_WhenUserHasRecoveryCode_ShouldClearAndUpdate()
    {
        // Arrange
        ApplicationUser identityUser = new() { Id = "user-1", UserName = "alex", RecoveryCode = "ABCDEF" };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.UpdateAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        await repository.ClearRecoveryCodeAsync("user-1", CancellationToken.None);

        // Assert
        identityUser.RecoveryCode.Should().BeNull();
        userManagerMock.Verify(manager => manager.UpdateAsync(identityUser), Times.Once);
    }

    [Fact]
    public async Task UnlockUserAsync_WhenUserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UnlockUserAsync("missing-id", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task UnlockUserAsync_WhenUserIsNotLockedOut_ShouldReturnNotLockedOutError()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            LockoutEnd = null,
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UnlockUserAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_locked_out");
    }

    [Fact]
    public async Task UnlockUserAsync_WhenLockoutEndIsInThePast_ShouldReturnNotLockedOutError()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(-1),
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UnlockUserAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_locked_out");
    }

    [Fact]
    public async Task UnlockUserAsync_WhenSetLockoutEndFails_ShouldReturnFailure()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10),
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.SetLockoutEndDateAsync(identityUser, null))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "LockoutError", Description = "Lockout error." }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UnlockUserAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        userManagerMock.Verify(manager => manager.ResetAccessFailedCountAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UnlockUserAsync_WhenResetAccessFailedCountFails_ShouldReturnFailure()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10),
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.SetLockoutEndDateAsync(identityUser, null))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ResetError", Description = "Reset error." }));

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UnlockUserAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UnlockUserAsync_WhenSucceeds_ShouldReturnMappedUser()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10),
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByIdAsync("user-1"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.SetLockoutEndDateAsync(identityUser, null))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await repository.UnlockUserAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-1");
        result.Value.UserName.Should().Be("alex");
        result.Value.Role.Should().Be(UserRole.Employee);
        userManagerMock.Verify(manager => manager.SetLockoutEndDateAsync(identityUser, null), Times.Once);
        userManagerMock.Verify(manager => manager.ResetAccessFailedCountAsync(identityUser), Times.Once);
    }

    [Fact]
    public async Task FindByIdentifierAsync_WhenLockoutEndIsExpired_ShouldMapUserAsNotLockedOut()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            EmailConfirmed = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddDays(-1),
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByNameAsync("alex"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync([UserRole.Employee.ToString()]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        UserAccount? result = await repository.FindByIdentifierAsync("alex", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public async Task FindByIdentifierAsync_WhenUserHasUnknownRole_ShouldMapRoleAsNone()
    {
        // Arrange
        ApplicationUser identityUser = new()
        {
            Id = "user-1",
            UserName = "alex",
            Email = "alex@example.com",
            EmailConfirmed = true,
        };

        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(manager => manager.FindByNameAsync("alex"))
            .ReturnsAsync(identityUser);
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(identityUser))
            .ReturnsAsync(["UnknownCustomRole"]);

        IdentityUserRepository repository = new(userManagerMock.Object, CreateDbContext());

        // Act
        UserAccount? result = await repository.FindByIdentifierAsync("alex", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Role.Should().Be(UserRole.None);
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

    private static ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext CreateSqliteContext()
    {
        Microsoft.Data.Sqlite.SqliteConnection connection = new("Filename=:memory:");
        connection.Open();

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        ApplicationDbContext context = new(options);
        context.Database.EnsureCreated();

        return context;
    }
}
