// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Colors;
using Beagl.Infrastructure.Colors.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Colors;

/// <summary>
/// Persists animal colors through Entity Framework Core.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class ColorRepository(ApplicationDbContext dbContext) : IColorRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <inheritdoc />
    public async Task<ColorsPage> GetPageAsync(GetColorsPageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<ColorEntity> queryable = _dbContext.Colors.AsNoTracking();

        int totalCount = await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

        List<ColorEntity> entities = await queryable
            .OrderBy(e => e.NameEn)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<Color> items = entities.ConvertAll(MapToDomain);

        return new ColorsPage(items, totalCount);
    }

    /// <inheritdoc />
    public async Task<Color?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ColorEntity? entity = await _dbContext.Colors
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string nameEn,
        string nameFr,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<ColorEntity> queryable = _dbContext.Colors
            .AsNoTracking()
            .Where(e => e.NameEn.Equals(nameEn, StringComparison.OrdinalIgnoreCase)
                || e.NameFr.Equals(nameFr, StringComparison.OrdinalIgnoreCase));

        if (excludeId.HasValue)
        {
            queryable = queryable.Where(e => e.Id != excludeId.Value);
        }

        return await queryable.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Check animal records when the Animal entity is implemented.
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<Color> CreateAsync(Color color, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(color);

        ColorEntity entity = new()
        {
            Id = color.Id,
            NameEn = color.NameEn,
            NameFr = color.NameFr,
        };

        _dbContext.Colors.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<Color> UpdateAsync(Color color, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(color);

        ColorEntity existing = await _dbContext.Colors
            .FirstOrDefaultAsync(e => e.Id == color.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Color with id '{color.Id}' was not found.");

        existing.NameEn = color.NameEn;
        existing.NameFr = color.NameFr;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDomain(existing);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        ColorEntity existing = await _dbContext.Colors
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Color with id '{id}' was not found.");

        _dbContext.Colors.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Color MapToDomain(ColorEntity entity)
    {
        return new Color(entity.Id, entity.NameEn, entity.NameFr);
    }
}
