// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Colors.Dtos;
using Beagl.Domain.Results;

namespace Beagl.Application.Colors.Services;

/// <summary>
/// Provides management operations for animal colors.
/// </summary>
public interface IColorService
{
    /// <summary>
    /// Gets a paginated list of colors.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of colors.</returns>
    public Task<ColorsPageDto> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single color by its unique identifier.
    /// </summary>
    /// <param name="id">The color identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The color, or <see langword="null"/> if not found.</returns>
    public Task<ColorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new color.
    /// </summary>
    /// <param name="request">The color data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created color, or a failure result.</returns>
    public Task<Result<ColorDto>> CreateAsync(SaveColorRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing color.
    /// </summary>
    /// <param name="id">The identifier of the color to update.</param>
    /// <param name="request">The updated color data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated color, or a failure result.</returns>
    public Task<Result<ColorDto>> UpdateAsync(Guid id, SaveColorRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a color by its unique identifier.
    /// </summary>
    /// <param name="id">The color identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A success result, or a failure result if the color is in use or not found.</returns>
    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
