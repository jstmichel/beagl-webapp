namespace Beagl.Infrastructure.Database;

/// <summary>
/// Abstraction for applying database migrations, allowing for
/// different implementations (e.g., EF Core, Dapper, etc.).
/// </summary>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Applies any pending migrations to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task MigrateAsync(CancellationToken cancellationToken = default);
}
