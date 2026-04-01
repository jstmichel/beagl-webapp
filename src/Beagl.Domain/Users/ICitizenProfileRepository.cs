// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;

namespace Beagl.Domain.Users;

/// <summary>
/// Provides persistence operations for citizen profiles.
/// </summary>
public interface ICitizenProfileRepository
{
    /// <summary>
    /// Retrieves a citizen profile by the associated user identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching citizen profile if found; otherwise, <see langword="null"/>.</returns>
    public Task<CitizenProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing citizen profile.
    /// </summary>
    /// <param name="profile">The profile data to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated profile or a failure result.</returns>
    public Task<Result<CitizenProfile>> UpdateAsync(UpdateCitizenProfile profile, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a citizen profile is complete for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the profile is complete; otherwise, <see langword="false"/>.</returns>
    public Task<bool> IsProfileCompleteAsync(string userId, CancellationToken cancellationToken);
}
