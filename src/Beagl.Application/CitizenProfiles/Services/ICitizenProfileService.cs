// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.CitizenProfiles.Dtos;
using Beagl.Domain.Results;

namespace Beagl.Application.CitizenProfiles.Services;

/// <summary>
/// Provides citizen profile management workflows for the application.
/// </summary>
public interface ICitizenProfileService
{
    /// <summary>
    /// Retrieves the citizen profile for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The citizen profile or a failure result.</returns>
    public Task<Result<CitizenProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the citizen profile for the specified user.
    /// </summary>
    /// <param name="request">The profile update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated profile or a failure result.</returns>
    public Task<Result<CitizenProfileDto>> UpdateProfileAsync(
        UpdateCitizenProfileRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the citizen profile is complete for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the profile is complete; otherwise, <see langword="false"/>.</returns>
    public Task<bool> IsProfileCompleteAsync(string userId, CancellationToken cancellationToken);
}
