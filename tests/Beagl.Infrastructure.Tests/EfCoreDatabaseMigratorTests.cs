// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Threading;
using System.Threading.Tasks;
using Beagl.Infrastructure;
using Beagl.Infrastructure.Database;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace Beagl.Infrastructure.Tests;

/// <summary>
/// Unit tests for <see cref="EfCoreDatabaseMigrator"/>.
/// </summary>
public class EfCoreDatabaseMigratorTests
{
    [Fact]
    public async Task MigrateAsync_WithRelationalDbContext_ShouldCompleteWithoutThrowing()
    {
        // Arrange
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using ApplicationDbContext dbContext = new(options);
        EfCoreDatabaseMigrator migrator = new(dbContext);

        // Act
        Func<Task> act = async () => await migrator.MigrateAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_NullDbContext_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _ = new EfCoreDatabaseMigrator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
