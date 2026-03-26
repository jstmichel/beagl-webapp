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
    /// Retrieves global users metrics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The global users metrics.</returns>
    public Task<UsersMetricsDto> GetUsersMetricsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated users list.
    /// </summary>
    /// <param name="request">The paginated request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated users result.</returns>
    public Task<UsersPageDto> GetUsersPageAsync(GetUsersPageRequest request, CancellationToken cancellationToken);

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

    /// <summary>
    /// Confirms an existing user account.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The confirmed user or a failure result.</returns>
    public Task<Result<UserDetailsDto>> ConfirmAccountAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Generates an email confirmation token for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated token or a failure result.</returns>
    public Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Confirms a user account using an email confirmation token.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the confirmation.</returns>
    public Task<Result> ConfirmAccountByTokenAsync(string userId, string token, CancellationToken cancellationToken);
}
