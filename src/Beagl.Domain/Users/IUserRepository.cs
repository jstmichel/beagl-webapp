// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;

namespace Beagl.Domain.Users;

/// <summary>
/// Provides persistence operations for application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of users.</returns>
    public Task<IReadOnlyList<UserAccount>> GetAllAsync(CancellationToken cancellationToken);

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
