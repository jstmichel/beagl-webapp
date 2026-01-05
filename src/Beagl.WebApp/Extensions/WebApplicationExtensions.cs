// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure;

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

        await DatabaseInitializer
            .InitializeAsync(app.Services, configuration)
            .ConfigureAwait(false);
    }
}
