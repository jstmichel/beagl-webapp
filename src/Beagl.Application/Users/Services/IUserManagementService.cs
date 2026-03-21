// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Dtos;
using Beagl.Domain.Results;

namespace Beagl.Application.Users.Services;

/// <summary>
/// Provides user management workflows for the application.
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of users.</returns>
    public Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a user by identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching user or a failure result.</returns>
    public Task<Result<UserDetailsDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user or a failure result.</returns>
    public Task<Result<UserDetailsDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="request">The user update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user or a failure result.</returns>
    public Task<Result<UserDetailsDto>> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an existing user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the deletion.</returns>
    public Task<Result> DeleteAsync(string userId, CancellationToken cancellationToken);
}
