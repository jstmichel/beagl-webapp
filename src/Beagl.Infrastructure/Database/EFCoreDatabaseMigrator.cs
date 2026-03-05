// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Database;

/// <summary>
/// Entity Framework Core implementation of the database migrator.
/// </summary>
/// <param name="dbContext">The application database context.</param>

public class EfCoreDatabaseMigrator(ApplicationDbContext dbContext) : IDatabaseMigrator
{
    /// <summary>
    /// Applies any pending migrations to the database using Entity Framework Core's migration system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        return dbContext.Database.MigrateAsync(cancellationToken);
    }
}
