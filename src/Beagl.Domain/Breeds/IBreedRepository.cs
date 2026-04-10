// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Breeds;

/// <summary>
/// Provides persistence operations for animal breeds.
/// </summary>
public interface IBreedRepository
{
    /// <summary>
    /// Gets a paginated list of breeds matching the specified query.
    /// </summary>
    /// <param name="query">The query parameters including pagination and optional filters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of breeds.</returns>
    public Task<BreedsPage> GetPageAsync(GetBreedsPageQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single breed by its unique identifier.
    /// </summary>
    /// <param name="id">The breed identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The breed, or <see langword="null"/> if not found.</returns>
    public Task<Breed?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a breed with the same animal type and name already exists.
    /// </summary>
    /// <param name="animalType">The animal type to check.</param>
    /// <param name="nameEn">The English name to check.</param>
    /// <param name="nameFr">The French name to check.</param>
    /// <param name="excludeId">An optional breed identifier to exclude from the check (used on updates).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if a duplicate exists; otherwise <see langword="false"/>.</returns>
    public Task<bool> ExistsAsync(AnimalType animalType, string nameEn, string nameFr, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new breed.
    /// </summary>
    /// <param name="breed">The breed to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created breed.</returns>
    public Task<Breed> CreateAsync(Breed breed, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing breed.
    /// </summary>
    /// <param name="breed">The breed with updated values.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated breed.</returns>
    public Task<Breed> UpdateAsync(Breed breed, CancellationToken cancellationToken);
}
