// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Setup.Dtos;
using Beagl.Domain.Results;

namespace Beagl.Application.Setup.Services;

/// <summary>
/// Provides first-run application setup workflows.
/// </summary>
public interface IInitialSetupService
{
    /// <summary>
    /// Determines whether first-run setup is still required.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when setup is required; otherwise, <see langword="false"/>.</returns>
    public Task<bool> IsSetupRequiredAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Completes first-run setup by creating the initial verified administrator.
    /// </summary>
    /// <param name="request">The setup request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The setup result.</returns>
    public Task<Result> CompleteInitialSetupAsync(CompleteInitialSetupRequest request, CancellationToken cancellationToken);
}
