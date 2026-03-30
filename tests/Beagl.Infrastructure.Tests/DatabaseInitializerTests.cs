// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Database;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace Beagl.Infrastructure.Tests;

public class DatabaseInitializerTests
{

    private static Mock<RoleManager<ApplicationRole>> CreateRoleManagerMock()
    {
        Mock<IRoleStore<ApplicationRole>> roleStoreMock = new();
        return new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object,
            new List<IRoleValidator<ApplicationRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Logger<RoleManager<ApplicationRole>>(new LoggerFactory()));
    }

    // Helper stub context to override Database property
    private class StubDbContext(
        DatabaseFacade databaseFacade)
        : ApplicationDbContext(new DbContextOptions<ApplicationDbContext>())
    {
        private readonly DatabaseFacade _databaseFacade = databaseFacade;

        public override DatabaseFacade Database => _databaseFacade;
    }

    [Fact]
    public async Task InitializeAsync_MigrateTrue_CallsMigrateAsync()
    {
        // Arrange
        Mock<IDatabaseMigrator> migratorMock = new();
        migratorMock.Setup(m => m.MigrateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        DatabaseInitializer initializer = new(migratorMock.Object, roleManagerMock.Object);

        // Act
        await initializer.InitializeAsync(migrate: true);

        // Assert
        migratorMock.Verify(m => m.MigrateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task InitializeAsync_MigrateFalse_DoesNotCallMigrateAsync()
    {
        // Arrange
        Mock<IDatabaseMigrator> migratorMock = new();
        migratorMock.Setup(m => m.MigrateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        DatabaseInitializer initializer = new(migratorMock.Object, roleManagerMock.Object);

        // Act
        await initializer.InitializeAsync(migrate: false);

        // Assert
        migratorMock.Verify(m => m.MigrateAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_NullDatabaseMigrator_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        DatabaseInitializer initializer = new(null!, roleManagerMock.Object);

        // Act
        Func<Task> act = async () => await initializer.InitializeAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InitializeAsync_NullRoleManager_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<IDatabaseMigrator> migratorMock = new();
        DatabaseInitializer initializer = new(migratorMock.Object, null!);

        // Act
        Func<Task> act = async () => await initializer.InitializeAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InitializeAsync_MissingRoles_CreatesAllDefaultRoles()
    {
        // Arrange
        Mock<IDatabaseMigrator> migratorMock = new();
        migratorMock.Setup(m => m.MigrateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>())).ReturnsAsync(IdentityResult.Success);

        DatabaseInitializer initializer = new(migratorMock.Object, roleManagerMock.Object);

        // Act
        await initializer.InitializeAsync(migrate: false);

        // Assert
        roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "Citizen")), Times.Once);
        roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "Employee")), Times.Once);
        roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "Administrator")), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenRoleAlreadyExists_ShouldNotCreateRole()
    {
        // Arrange
        Mock<IDatabaseMigrator> migratorMock = new();
        migratorMock.Setup(m => m.MigrateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        DatabaseInitializer initializer = new(migratorMock.Object, roleManagerMock.Object);

        // Act
        await initializer.InitializeAsync(migrate: false);

        // Assert
        roleManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationRole>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_WhenRoleCreationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Mock<IDatabaseMigrator> migratorMock = new();
        migratorMock.Setup(m => m.MigrateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleCreationFailed", Description = "failed" }));

        DatabaseInitializer initializer = new(migratorMock.Object, roleManagerMock.Object);

        // Act
        Func<Task> act = async () => await initializer.InitializeAsync(migrate: false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Citizen*");
    }

    [Fact]
    public async Task InitializeAsync_ShouldForwardCancellationTokenToMigrator()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Mock<IDatabaseMigrator> migratorMock = new();
        migratorMock.Setup(m => m.MigrateAsync(cancellationToken)).Returns(Task.CompletedTask);

        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        DatabaseInitializer initializer = new(migratorMock.Object, roleManagerMock.Object);

        // Act
        await initializer.InitializeAsync(migrate: true, cancellationToken);

        // Assert
        migratorMock.Verify(m => m.MigrateAsync(cancellationToken), Times.Once);
    }
}
