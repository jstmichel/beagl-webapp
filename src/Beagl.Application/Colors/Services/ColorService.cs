// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Colors.Dtos;
using Beagl.Domain;
using Beagl.Domain.Colors;
using Beagl.Domain.Results;

namespace Beagl.Application.Colors.Services;

/// <summary>
/// Implements management operations for animal colors.
/// </summary>
/// <param name="repository">The color repository.</param>
public sealed class ColorService(IColorRepository repository) : IColorService
{
    private readonly IColorRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <inheritdoc />
    public async Task<ColorsPageDto> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        GetColorsPageQuery query = new(page, pageSize);
        ColorsPage result = await _repository.GetPageAsync(query, cancellationToken).ConfigureAwait(false);
        return MapToPageDto(result);
    }

    /// <inheritdoc />
    public async Task<ColorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Color? color = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return color is null ? null : MapToDto(color);
    }

    /// <inheritdoc />
    public async Task<Result<ColorDto>> CreateAsync(SaveColorRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<ColorDto>(validation.Error!);
        }

        bool duplicate = await _repository.ExistsAsync(request.NameEn.Trim(), request.NameFr.Trim(), null, cancellationToken).ConfigureAwait(false);
        if (duplicate)
        {
            return Result.Failure<ColorDto>(new ResultError("color.duplicate_name", "A color with the same name already exists."));
        }

        Color color = new(
            Guid.NewGuid(),
            request.NameEn.Trim(),
            request.NameFr.Trim());

        Color created = await _repository.CreateAsync(color, cancellationToken).ConfigureAwait(false);
        return Result.Success(MapToDto(created));
    }

    /// <inheritdoc />
    public async Task<Result<ColorDto>> UpdateAsync(Guid id, SaveColorRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<ColorDto>(validation.Error!);
        }

        Color? existing = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return Result.Failure<ColorDto>(new ResultError("color.not_found", "The color was not found."));
        }

        bool duplicate = await _repository.ExistsAsync(request.NameEn.Trim(), request.NameFr.Trim(), id, cancellationToken).ConfigureAwait(false);
        if (duplicate)
        {
            return Result.Failure<ColorDto>(new ResultError("color.duplicate_name", "A color with the same name already exists."));
        }

        Color updated = existing with
        {
            NameEn = request.NameEn.Trim(),
            NameFr = request.NameFr.Trim(),
        };

        Color saved = await _repository.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
        return Result.Success(MapToDto(saved));
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Color? existing = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return Result.Failure(new ResultError("color.not_found", "The color was not found."));
        }

        bool inUse = await _repository.IsInUseAsync(id, cancellationToken).ConfigureAwait(false);
        if (inUse)
        {
            return Result.Failure(new ResultError("color.in_use", "Cannot delete a color that is currently in use."));
        }

        await _repository.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private static Result ValidateRequest(SaveColorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NameEn))
        {
            return Result.Failure(new ResultError("color.name_en_required", "The English name is required."));
        }

        if (request.NameEn.Trim().Length > ValidationConstants.ColorNameMaxLength)
        {
            return Result.Failure(new ResultError("color.name_en_too_long", $"The English name must not exceed {ValidationConstants.ColorNameMaxLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(request.NameFr))
        {
            return Result.Failure(new ResultError("color.name_fr_required", "The French name is required."));
        }

        if (request.NameFr.Trim().Length > ValidationConstants.ColorNameMaxLength)
        {
            return Result.Failure(new ResultError("color.name_fr_too_long", $"The French name must not exceed {ValidationConstants.ColorNameMaxLength} characters."));
        }

        return Result.Success();
    }

    private static ColorDto MapToDto(Color color)
    {
        return new ColorDto(
            color.Id,
            color.NameEn,
            color.NameFr);
    }

    private static ColorsPageDto MapToPageDto(ColorsPage page)
    {
        IReadOnlyList<ColorDto> items = [..page.Items.Select(MapToDto)];
        return new ColorsPageDto(items, page.TotalCount);
    }
}
