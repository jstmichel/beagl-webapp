// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
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
    /// Applies any pending database migrations and seeds initial data.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    [ExcludeFromCodeCoverage]
    internal static async Task ExecuteMigrationsAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        RoleManager<ApplicationRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        EfCoreDatabaseMigrator migrator = new(dbContext);

        DatabaseInitializer initializer = new(migrator, roleManager);
        await initializer.InitializeAsync().ConfigureAwait(false);
    }
}
