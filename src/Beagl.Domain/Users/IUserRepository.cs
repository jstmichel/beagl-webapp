// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;

namespace Beagl.Domain.Users;

/// <summary>
/// Provides persistence operations for application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Determines whether at least one administrator account exists.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when an administrator exists; otherwise, <see langword="false"/>.</returns>
    public Task<bool> HasAnyAdministratorAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves global users metrics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The global users metrics.</returns>
    public Task<UsersMetrics> GetMetricsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated users list.
    /// </summary>
    /// <param name="query">The paginated query options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated users result.</returns>
    public Task<UsersPage> GetPageAsync(GetUsersPageQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a user by identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching user if found; otherwise, <see langword="null"/>.</returns>
    public Task<UserAccount?> GetByIdAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user or a failure result.</returns>
    public Task<Result<UserAccount>> CreateAsync(CreateUserAccount user, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">The user data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user or a failure result.</returns>
    public Task<Result<UserAccount>> UpdateAsync(UpdateUserAccount user, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an existing user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the deletion.</returns>
    public Task<Result> DeleteAsync(string userId, CancellationToken cancellationToken);
}
