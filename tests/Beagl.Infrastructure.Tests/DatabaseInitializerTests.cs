// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Database;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
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

        Mock<IConfiguration> configurationMock = new();
        DatabaseInitializer initializer = new(migratorMock.Object, configurationMock.Object, roleManagerMock.Object);

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

        Mock<IConfiguration> configurationMock = new();
        DatabaseInitializer initializer = new(migratorMock.Object, configurationMock.Object, roleManagerMock.Object);

        // Act
        await initializer.InitializeAsync(migrate: false);

        // Assert
        migratorMock.Verify(m => m.MigrateAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<IDatabaseMigrator> migratorMock = new();
        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        DatabaseInitializer initializer = new(migratorMock.Object, null!, roleManagerMock.Object);

        // Act
        Func<Task> act = async () => await initializer.InitializeAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InitializeAsync_NullDatabaseMigrator_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<IConfiguration> configurationMock = new();
        Mock<RoleManager<ApplicationRole>> roleManagerMock = CreateRoleManagerMock();
        DatabaseInitializer initializer = new(null!, configurationMock.Object, roleManagerMock.Object);

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

        Mock<IConfiguration> configurationMock = new();
        DatabaseInitializer initializer = new(migratorMock.Object, configurationMock.Object, roleManagerMock.Object);

        // Act
        await initializer.InitializeAsync(migrate: false);

        // Assert
        roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "Citizen")), Times.Once);
        roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "Employee")), Times.Once);
        roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "Administrator")), Times.Once);
    }
}
