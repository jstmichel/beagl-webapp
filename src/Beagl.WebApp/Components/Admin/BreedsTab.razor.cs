// MIT License - Copyright (c) 2025 Jonathan St-Michel

// Blazor event handlers catch all exceptions to prevent SignalR circuit failures.
#pragma warning disable CA1031 // Do not catch general exception types

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Beagl.Application.Breeds.Dtos;
using Beagl.Application.Breeds.Services;
using Beagl.Domain.Breeds;
using Beagl.Domain.Results;
using Beagl.WebApp.Extensions;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Admin;

/// <summary>
/// Code-behind for the animal breeds management tab component.
/// </summary>
public sealed partial class BreedsTab : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<BreedDto> _breeds = [];
    private readonly List<string> _formErrors = [];
    private readonly BreedFormModel _form = new();
    private const int _pageSize = 10;

    private bool _isLoading = true;
    private bool _isSaving;
    private bool _showModal;
    private int _currentPage = 1;
    private int _totalCount;
    private string _animalTypeFilterRaw = string.Empty;
    private string _isActiveFilterRaw = string.Empty;
    private Guid? _editingBreedId;
    private string? _statusMessage;
    private string? _errorMessage;

    [Inject]
    private IBreedService BreedService { get; set; } = default!;

    [Inject]
    private IStringLocalizer<BreedsResource> L { get; set; } = default!;

    [Inject]
    private ILogger<BreedsTab> Logger { get; set; } = default!;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_totalCount / _pageSize));

    private int PageStartIndex => _totalCount == 0 ? 0 : ((_currentPage - 1) * _pageSize) + 1;

    private int PageEndIndex => _totalCount == 0 ? 0 : Math.Min(_currentPage * _pageSize, _totalCount);

    private bool CanGoPreviousPage => _currentPage > 1;

    private bool CanGoNextPage => _currentPage < TotalPages;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await LoadBreedsAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private AnimalType? ParseAnimalTypeFilter()
    {
        if (string.IsNullOrEmpty(_animalTypeFilterRaw))
        {
            return null;
        }

        if (int.TryParse(_animalTypeFilterRaw, out int value) && Enum.IsDefined(typeof(AnimalType), value))
        {
            return (AnimalType)value;
        }

        return null;
    }

    private bool? ParseIsActiveFilter()
    {
        if (string.IsNullOrEmpty(_isActiveFilterRaw))
        {
            return null;
        }

        if (bool.TryParse(_isActiveFilterRaw, out bool value))
        {
            return value;
        }

        return null;
    }

    private async Task LoadBreedsAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            AnimalType? animalTypeFilter = ParseAnimalTypeFilter();
            bool? isActiveFilter = ParseIsActiveFilter();

            BreedsPageDto page = await BreedService
                .GetPageAsync(_currentPage, _pageSize, animalTypeFilter, isActiveFilter, _cts.Token)
                .ConfigureAwait(false);

            _breeds.Clear();
            _breeds.AddRange(page.Items);
            _totalCount = page.TotalCount;
        }
        catch (Exception exception)
        {
            LogLoadBreedsFailed(Logger, exception);
            _errorMessage = L["Breeds.Error.LoadFailed"];
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OnFilterChangedAsync()
    {
        _currentPage = 1;
        await LoadBreedsAsync().ConfigureAwait(false);
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPreviousPage)
        {
            return;
        }

        _currentPage--;
        await LoadBreedsAsync().ConfigureAwait(false);
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNextPage)
        {
            return;
        }

        _currentPage++;
        await LoadBreedsAsync().ConfigureAwait(false);
    }

    private void OpenCreateModal()
    {
        _editingBreedId = null;
        _form.Reset();
        _formErrors.Clear();
        _showModal = true;
        _statusMessage = null;
        _errorMessage = null;
    }

    private void OpenEditModal(BreedDto breed)
    {
        _editingBreedId = breed.Id;
        _form.AnimalType = breed.AnimalType;
        _form.NameEn = breed.NameEn;
        _form.NameFr = breed.NameFr;
        _form.DescriptionEn = breed.DescriptionEn;
        _form.DescriptionFr = breed.DescriptionFr;
        _formErrors.Clear();
        _showModal = true;
        _statusMessage = null;
        _errorMessage = null;
    }

    private void CloseModal()
    {
        _showModal = false;
        _formErrors.Clear();
    }

    private async Task SaveBreedAsync()
    {
        _isSaving = true;
        _formErrors.Clear();

        try
        {
            SaveBreedRequest request = new(
                _form.AnimalType,
                _form.NameEn,
                _form.NameFr,
                _form.DescriptionEn,
                _form.DescriptionFr);

            if (_editingBreedId is null)
            {
                Result<BreedDto> result = await BreedService.CreateAsync(request, _cts.Token).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    _formErrors.Add(L.LocalizeError(result.Error!));
                    _isSaving = false;
                    return;
                }

                _showModal = false;
                _statusMessage = L["Breeds.Success.Created"];
            }
            else
            {
                Result<BreedDto> result = await BreedService
                    .UpdateAsync(_editingBreedId.Value, request, _cts.Token)
                    .ConfigureAwait(false);

                if (result.IsFailure)
                {
                    _formErrors.Add(L.LocalizeError(result.Error!));
                    _isSaving = false;
                    return;
                }

                _showModal = false;
                _statusMessage = L["Breeds.Success.Updated"];
            }

            await LoadBreedsAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogSaveBreedFailed(Logger, exception);
            _formErrors.Add(L["Breeds.Error.UnexpectedError"]);
        }

        _isSaving = false;
    }

    private async Task ToggleActiveAsync(Guid breedId)
    {
        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;

        try
        {
            Result result = await BreedService.ToggleActiveAsync(breedId, _cts.Token).ConfigureAwait(false);

            if (result.IsFailure)
            {
                _errorMessage = L.LocalizeError(result.Error!);
                _isSaving = false;
                return;
            }

            _statusMessage = L["Breeds.Success.StatusChanged"];
            await LoadBreedsAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogToggleBreedActiveFailed(Logger, exception);
            _errorMessage = L["Breeds.Error.UnexpectedError"];
        }

        _isSaving = false;
    }

    private static string LocalizeName(string en, string fr)
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? fr : en;
    }

    private string LocalizeAnimalType(AnimalType animalType)
    {
        LocalizedString localized = L[$"Breeds.AnimalType.{animalType}"];
        return localized.ResourceNotFound ? animalType.ToString() : localized.Value;
    }

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to load breeds")]
    private static partial void LogLoadBreedsFailed(ILogger logger, Exception exception);

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to save breed")]
    private static partial void LogSaveBreedFailed(ILogger logger, Exception exception);

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to toggle breed active status")]
    private static partial void LogToggleBreedActiveFailed(ILogger logger, Exception exception);

    private sealed class BreedFormModel
    {
        public AnimalType AnimalType { get; set; }

        public string NameEn { get; set; } = string.Empty;

        public string NameFr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;

        public string DescriptionFr { get; set; } = string.Empty;

        public void Reset()
        {
            AnimalType = AnimalType.None;
            NameEn = string.Empty;
            NameFr = string.Empty;
            DescriptionEn = string.Empty;
            DescriptionFr = string.Empty;
        }
    }
}
