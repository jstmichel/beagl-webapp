// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Threading;
using System.Threading.Tasks;
using Beagl.Infrastructure.Database;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
    public async Task MigrateAsync_NullDbContext_ThrowsArgumentNullException()
    {
        // Arrange
        EfCoreDatabaseMigrator? migrator = null;
        // Act
        Func<Task> act = async () =>
        {
            migrator = new(null!);
            await migrator.MigrateAsync();
        };
        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
