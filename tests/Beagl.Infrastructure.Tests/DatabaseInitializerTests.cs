// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Beagl.Infrastructure.Tests;

public class DatabaseInitializerTests
{

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

        Mock<IConfiguration> configurationMock = new();
        DatabaseInitializer initializer = new(migratorMock.Object, configurationMock.Object);

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

        Mock<IConfiguration> configurationMock = new();
        DatabaseInitializer initializer = new(migratorMock.Object, configurationMock.Object);

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
        DatabaseInitializer initializer = new(migratorMock.Object, null!);

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
        DatabaseInitializer initializer = new(null!, configurationMock.Object);

        // Act
        Func<Task> act = async () => await initializer.InitializeAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
