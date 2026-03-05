// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Database;
using Microsoft.Extensions.Configuration;

namespace Beagl.Infrastructure;

/// <summary>
/// Provides methods to initialize the application's database, apply migrations, and seed default roles and users.
/// </summary>
/// <param name="databaseMigrator">The database migrator abstraction.</param>
/// <param name="configuration">The configuration containing seed data.</param>
public class DatabaseInitializer(
    IDatabaseMigrator databaseMigrator,
    IConfiguration configuration)
{
    /// <summary>
    /// Applies pending migrations (if <paramref name="migrate"/> is true) and seeds default roles and users using configuration values.
    /// </summary>
    /// <param name="migrate">If true, applies pending migrations before seeding data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync(
        bool migrate = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseMigrator, nameof(databaseMigrator));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        if (migrate)
        {
            await databaseMigrator.MigrateAsync(cancellationToken);
        }
    }
}
