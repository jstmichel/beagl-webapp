// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure;
using Beagl.Infrastructure.Database;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Beagl.WebApp.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="WebApplication"/> class to support application startup tasks.
/// </summary>
internal static class WebApplicationExtensions
{
    /// <summary>
    /// Applies any pending database migrations and seeds initial data using the provided configuration.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="configuration">The application configuration containing seed data and other settings.</param>
    internal static async Task ExecuteMigrationsAsync(this WebApplication app, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configuration);

        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        RoleManager<ApplicationRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        EfCoreDatabaseMigrator migrator = new(dbContext);

        DatabaseInitializer initializer = new(migrator, configuration, roleManager);
        await initializer.InitializeAsync().ConfigureAwait(false);
    }
}
