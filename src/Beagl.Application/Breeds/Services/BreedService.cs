// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Breeds.Dtos;
using Beagl.Domain;
using Beagl.Domain.Breeds;
using Beagl.Domain.Results;

namespace Beagl.Application.Breeds.Services;

/// <summary>
/// Implements management operations for animal breeds.
/// </summary>
/// <param name="repository">The breed repository.</param>
public sealed class BreedService(IBreedRepository repository) : IBreedService
{
    private readonly IBreedRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <inheritdoc />
    public async Task<BreedsPageDto> GetPageAsync(int page, int pageSize, AnimalType? animalTypeFilter, bool? isActiveFilter, CancellationToken cancellationToken)
    {
        GetBreedsPageQuery query = new(page, pageSize, animalTypeFilter, isActiveFilter);
        BreedsPage result = await _repository.GetPageAsync(query, cancellationToken).ConfigureAwait(false);
        return MapToPageDto(result);
    }

    /// <inheritdoc />
    public async Task<BreedDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Breed? breed = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return breed is null ? null : MapToDto(breed);
    }

    /// <inheritdoc />
    public async Task<Result<BreedDto>> CreateAsync(SaveBreedRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<BreedDto>(validation.Error!);
        }

        bool duplicate = await _repository.ExistsAsync(request.AnimalType, request.NameEn.Trim(), request.NameFr.Trim(), null, cancellationToken).ConfigureAwait(false);
        if (duplicate)
        {
            return Result.Failure<BreedDto>(new ResultError("breed.duplicate_name", "A breed with the same animal type and name already exists."));
        }

        Breed breed = new(
            Guid.NewGuid(),
            request.AnimalType,
            request.NameEn.Trim(),
            request.NameFr.Trim(),
            true);

        Breed created = await _repository.CreateAsync(breed, cancellationToken).ConfigureAwait(false);
        return Result.Success(MapToDto(created));
    }

    /// <inheritdoc />
    public async Task<Result<BreedDto>> UpdateAsync(Guid id, SaveBreedRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<BreedDto>(validation.Error!);
        }

        Breed? existing = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return Result.Failure<BreedDto>(new ResultError("breed.not_found", "The breed was not found."));
        }

        bool duplicate = await _repository.ExistsAsync(request.AnimalType, request.NameEn.Trim(), request.NameFr.Trim(), id, cancellationToken).ConfigureAwait(false);
        if (duplicate)
        {
            return Result.Failure<BreedDto>(new ResultError("breed.duplicate_name", "A breed with the same animal type and name already exists."));
        }

        Breed updated = existing with
        {
            AnimalType = request.AnimalType,
            NameEn = request.NameEn.Trim(),
            NameFr = request.NameFr.Trim(),
        };

        Breed saved = await _repository.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
        return Result.Success(MapToDto(saved));
    }

    /// <inheritdoc />
    public async Task<Result> ToggleActiveAsync(Guid id, CancellationToken cancellationToken)
    {
        Breed? existing = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return Result.Failure(new ResultError("breed.not_found", "The breed was not found."));
        }

        Breed toggled = existing with { IsActive = !existing.IsActive };
        await _repository.UpdateAsync(toggled, cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private static Result ValidateRequest(SaveBreedRequest request)
    {
        if (!Enum.IsDefined(request.AnimalType))
        {
            return Result.Failure(new ResultError("breed.animal_type_required", "A valid animal type is required."));
        }

        if (string.IsNullOrWhiteSpace(request.NameEn))
        {
            return Result.Failure(new ResultError("breed.name_en_required", "The English name is required."));
        }

        if (request.NameEn.Trim().Length > ValidationConstants.BreedNameMaxLength)
        {
            return Result.Failure(new ResultError("breed.name_en_too_long", $"The English name must not exceed {ValidationConstants.BreedNameMaxLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(request.NameFr))
        {
            return Result.Failure(new ResultError("breed.name_fr_required", "The French name is required."));
        }

        if (request.NameFr.Trim().Length > ValidationConstants.BreedNameMaxLength)
        {
            return Result.Failure(new ResultError("breed.name_fr_too_long", $"The French name must not exceed {ValidationConstants.BreedNameMaxLength} characters."));
        }

        return Result.Success();
    }

    private static BreedDto MapToDto(Breed breed)
    {
        return new BreedDto(
            breed.Id,
            breed.AnimalType,
            breed.NameEn,
            breed.NameFr,
            breed.IsActive);
    }

    private static BreedsPageDto MapToPageDto(BreedsPage page)
    {
        IReadOnlyList<BreedDto> items = [..page.Items.Select(MapToDto)];
        return new BreedsPageDto(items, page.TotalCount);
    }
}
