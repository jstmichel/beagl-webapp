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

    /// <summary>
    /// Confirms an existing user account.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The confirmed user or a failure result.</returns>
    public Task<Result<UserAccount>> ConfirmAccountAsync(string userId, CancellationToken cancellationToken);

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

    /// <summary>
    /// Self-registers a new Citizen account.
    /// </summary>
    /// <param name="account">The citizen registration data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user or a failure result.</returns>
    public Task<Result<UserAccount>> RegisterCitizenAsync(RegisterCitizenAccount account, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a user by username or email identifier.
    /// </summary>
    /// <param name="identifier">The username or email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching user if found; otherwise, <see langword="null"/>.</returns>
    public Task<UserAccount?> FindByIdentifierAsync(string identifier, CancellationToken cancellationToken);

    /// <summary>
    /// Generates a recovery code for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the code generation.</returns>
    public Task<Result> GenerateRecoveryCodeAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Resets a user password using a recovery code.
    /// </summary>
    /// <param name="code">The recovery code.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the password reset.</returns>
    public Task<Result> ResetPasswordByRecoveryCodeAsync(string code, string newPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the identity contact information (email and phone number) for a citizen.
    /// </summary>
    /// <param name="identity">The identity contact data to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user account or a failure result.</returns>
    public Task<Result<UserAccount>> UpdateCitizenIdentityAsync(
        UpdateCitizenIdentity identity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears any active recovery code for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ClearRecoveryCodeAsync(string userId, CancellationToken cancellationToken);
}
