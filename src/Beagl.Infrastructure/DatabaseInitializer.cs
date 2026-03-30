// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Database;
using Beagl.Infrastructure.Users.Entities;
using Beagl.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Beagl.Infrastructure;

/// <summary>
/// Provides methods to initialize the application's database, apply migrations, and seed default roles and users.
/// </summary>
/// <param name="databaseMigrator">The database migrator abstraction.</param>
/// <param name="roleManager">The role manager used to ensure default roles exist.</param>
public class DatabaseInitializer(
    IDatabaseMigrator databaseMigrator,
    RoleManager<ApplicationRole> roleManager)
{
    private readonly IDatabaseMigrator _databaseMigrator = databaseMigrator ?? throw new ArgumentNullException(nameof(databaseMigrator));
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));

    private static readonly IReadOnlyList<string> _defaultRoles =
    [
        UserRole.Citizen.ToString(),
        UserRole.Employee.ToString(),
        UserRole.Administrator.ToString(),
    ];

    /// <summary>
    /// Applies pending migrations (if <paramref name="migrate"/> is true) and seeds default roles and users.
    /// </summary>
    /// <param name="migrate">If true, applies pending migrations before seeding data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync(
        bool migrate = true,
        CancellationToken cancellationToken = default)
    {
        if (migrate)
        {
            await _databaseMigrator.MigrateAsync(cancellationToken);
        }

        await EnsureRolesExistAsync().ConfigureAwait(false);
    }

    private async Task EnsureRolesExistAsync()
    {
        foreach (string roleName in _defaultRoles)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName).ConfigureAwait(false);
            if (roleExists)
            {
                continue;
            }

            IdentityResult roleCreationResult = await _roleManager.CreateAsync(new ApplicationRole { Name = roleName })
                .ConfigureAwait(false);
            if (!roleCreationResult.Succeeded)
            {
                throw new InvalidOperationException($"Could not create identity role '{roleName}'.");
            }
        }
    }
}
