// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Beagl.Infrastructure;

/// <summary>
/// Provides methods to initialize the application's database, apply migrations, and seed default roles and users.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending migrations (if <paramref name="migrate"/> is true) and seeds default roles and users using configuration values.
    /// </summary>
    /// <param name="serviceProvider">The application's service provider.</param>
    /// <param name="configuration">The configuration containing seed data.</param>
    /// <param name="migrate">If true, applies pending migrations before seeding data.</param>
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        bool migrate = true)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        using IServiceScope scope = serviceProvider.CreateScope();
        if (migrate)
        {
            ApplicationDbContext db = scope
                .ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();
        }
    }
}
