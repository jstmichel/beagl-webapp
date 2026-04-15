// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Breeds;
using Beagl.Infrastructure.Breeds.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Breeds;

/// <summary>
/// Persists animal breeds through Entity Framework Core.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class BreedRepository(ApplicationDbContext dbContext) : IBreedRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <inheritdoc />
    public async Task<BreedsPage> GetPageAsync(GetBreedsPageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<BreedEntity> queryable = _dbContext.Breeds.AsNoTracking();

        if (query.AnimalTypeFilter.HasValue)
        {
            queryable = queryable.Where(e => e.AnimalType == query.AnimalTypeFilter.Value);
        }

        if (query.IsActiveFilter.HasValue)
        {
            queryable = queryable.Where(e => e.IsActive == query.IsActiveFilter.Value);
        }

        int totalCount = await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

        List<BreedEntity> entities = await queryable
            .OrderBy(e => e.NameEn)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<Breed> items = entities.ConvertAll(MapToDomain);

        return new BreedsPage(items, totalCount);
    }

    /// <inheritdoc />
    public async Task<Breed?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        BreedEntity? entity = await _dbContext.Breeds
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        AnimalType animalType,
        string nameEn,
        string nameFr,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        IQueryable<BreedEntity> queryable = _dbContext.Breeds
            .AsNoTracking()
            .Where(e => e.AnimalType == animalType
                && (e.NameEn.Equals(nameEn, StringComparison.OrdinalIgnoreCase)
                    || e.NameFr.Equals(nameFr, StringComparison.OrdinalIgnoreCase)));

        if (excludeId.HasValue)
        {
            queryable = queryable.Where(e => e.Id != excludeId.Value);
        }

        return await queryable.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Breed> CreateAsync(Breed breed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(breed);

        BreedEntity entity = new()
        {
            Id = breed.Id,
            AnimalType = breed.AnimalType,
            NameEn = breed.NameEn,
            NameFr = breed.NameFr,
            IsActive = breed.IsActive,
        };

        _dbContext.Breeds.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<Breed> UpdateAsync(Breed breed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(breed);

        BreedEntity existing = await _dbContext.Breeds
            .FirstOrDefaultAsync(e => e.Id == breed.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Breed with id '{breed.Id}' was not found.");

        existing.AnimalType = breed.AnimalType;
        existing.NameEn = breed.NameEn;
        existing.NameFr = breed.NameFr;
        existing.IsActive = breed.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDomain(existing);
    }

    private static Breed MapToDomain(BreedEntity entity)
    {
        return new Breed(
            entity.Id,
            entity.AnimalType,
            entity.NameEn,
            entity.NameFr,
            entity.IsActive);
    }
}
