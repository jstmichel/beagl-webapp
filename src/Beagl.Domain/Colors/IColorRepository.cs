// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Colors;

/// <summary>
/// Provides persistence operations for animal colors.
/// </summary>
public interface IColorRepository
{
    /// <summary>
    /// Gets a paginated list of colors matching the specified query.
    /// </summary>
    /// <param name="query">The query parameters including pagination.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of colors.</returns>
    public Task<ColorsPage> GetPageAsync(GetColorsPageQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single color by its unique identifier.
    /// </summary>
    /// <param name="id">The color identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The color, or <see langword="null"/> if not found.</returns>
    public Task<Color?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a color with the same name already exists.
    /// </summary>
    /// <param name="nameEn">The English name to check.</param>
    /// <param name="nameFr">The French name to check.</param>
    /// <param name="excludeId">An optional color identifier to exclude from the check (used on updates).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if a duplicate exists; otherwise <see langword="false"/>.</returns>
    public Task<bool> ExistsAsync(string nameEn, string nameFr, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a color is currently in use by any animal record.
    /// </summary>
    /// <param name="id">The color identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the color is in use; otherwise <see langword="false"/>.</returns>
    public Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new color.
    /// </summary>
    /// <param name="color">The color to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created color.</returns>
    public Task<Color> CreateAsync(Color color, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing color.
    /// </summary>
    /// <param name="color">The color with updated values.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated color.</returns>
    public Task<Color> UpdateAsync(Color color, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a color by its unique identifier.
    /// </summary>
    /// <param name="id">The color identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
