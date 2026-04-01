// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.Infrastructure.Database;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Users;

/// <summary>
/// Persists citizen profiles through Entity Framework Core.
/// </summary>
/// <param name="applicationDbContext">The application database context.</param>
public sealed class CitizenProfileRepository(
    ApplicationDbContext applicationDbContext) : ICitizenProfileRepository
{
    private readonly ApplicationDbContext _applicationDbContext =
        applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));

    /// <inheritdoc />
    public async Task<CitizenProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        CitizenProfileEntity? entity = await _applicationDbContext.CitizenProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<Result<CitizenProfile>> UpdateAsync(
        UpdateCitizenProfile profile,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(profile);
        cancellationToken.ThrowIfCancellationRequested();

        CitizenProfileEntity? entity = await _applicationDbContext.CitizenProfiles
            .SingleOrDefaultAsync(p => p.UserId == profile.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return Result.Failure<CitizenProfile>(
                new ResultError("citizen_profile.not_found", "The citizen profile could not be found."));
        }

        entity.FirstName = profile.FirstName;
        entity.LastName = profile.LastName;
        entity.Address = new AddressEntity
        {
            Street = profile.Address.Street,
            City = profile.Address.City,
            Province = profile.Address.Province,
            PostalCode = profile.Address.PostalCode,
        };
        entity.DateOfBirth = profile.DateOfBirth;
        entity.CommunicationPreference = profile.CommunicationPreference;
        entity.LanguagePreference = profile.LanguagePreference;

        await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(MapToDomain(entity));
    }

    /// <inheritdoc />
    public async Task<bool> IsProfileCompleteAsync(string userId, CancellationToken cancellationToken)
    {
        CitizenProfileEntity? entity = await _applicationDbContext.CitizenProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(entity.FirstName)
            && !string.IsNullOrWhiteSpace(entity.LastName)
            && entity.Address is not null
            && !string.IsNullOrWhiteSpace(entity.Address.Street)
            && !string.IsNullOrWhiteSpace(entity.Address.City)
            && !string.IsNullOrWhiteSpace(entity.Address.Province)
            && !string.IsNullOrWhiteSpace(entity.Address.PostalCode)
            && entity.DateOfBirth.HasValue
            && entity.CommunicationPreference != CommunicationPreference.None
            && entity.LanguagePreference != LanguagePreference.None;
    }

    private static CitizenProfile MapToDomain(CitizenProfileEntity entity)
    {
        Address? address = entity.Address is not null
            ? new Address(
                entity.Address.Street,
                entity.Address.City,
                entity.Address.Province,
                entity.Address.PostalCode)
            : null;

        return new CitizenProfile(
            entity.Id,
            entity.UserId,
            entity.FirstName,
            entity.LastName,
            address,
            entity.DateOfBirth,
            entity.CommunicationPreference,
            entity.LanguagePreference);
    }
}
