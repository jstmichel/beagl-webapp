// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Breeds.Dtos;
using Beagl.Domain.Breeds;
using Beagl.Domain.Results;

namespace Beagl.Application.Breeds.Services;

/// <summary>
/// Provides management operations for animal breeds.
/// </summary>
public interface IBreedService
{
    /// <summary>
    /// Gets a paginated list of breeds.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of items per page.</param>
    /// <param name="animalTypeFilter">An optional animal type filter.</param>
    /// <param name="isActiveFilter">An optional active status filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of breeds.</returns>
    public Task<BreedsPageDto> GetPageAsync(int page, int pageSize, AnimalType? animalTypeFilter, bool? isActiveFilter, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single breed by its unique identifier.
    /// </summary>
    /// <param name="id">The breed identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The breed, or <see langword="null"/> if not found.</returns>
    public Task<BreedDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new breed.
    /// </summary>
    /// <param name="request">The breed data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created breed, or a failure result.</returns>
    public Task<Result<BreedDto>> CreateAsync(SaveBreedRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing breed.
    /// </summary>
    /// <param name="id">The identifier of the breed to update.</param>
    /// <param name="request">The updated breed data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated breed, or a failure result.</returns>
    public Task<Result<BreedDto>> UpdateAsync(Guid id, SaveBreedRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Toggles the active status of a breed.
    /// </summary>
    /// <param name="id">The identifier of the breed to toggle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A success result, or a failure result if the breed is not found.</returns>
    public Task<Result> ToggleActiveAsync(Guid id, CancellationToken cancellationToken);
}
