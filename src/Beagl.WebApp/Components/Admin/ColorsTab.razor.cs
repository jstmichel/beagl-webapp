// MIT License - Copyright (c) 2025 Jonathan St-Michel

// Blazor event handlers catch all exceptions to prevent SignalR circuit failures.
#pragma warning disable CA1031 // Do not catch general exception types

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Beagl.Application.Colors.Dtos;
using Beagl.Application.Colors.Services;
using Beagl.Domain.Results;
using Beagl.WebApp.Extensions;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Admin;

/// <summary>
/// Code-behind for the animal colors management tab component.
/// </summary>
public sealed partial class ColorsTab : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<ColorDto> _colors = [];
    private readonly List<string> _formErrors = [];
    private readonly ColorFormModel _form = new();
    private const int _pageSize = 10;

    private bool _isLoading = true;
    private bool _isSaving;
    private bool _showModal;
    private bool _showDeleteConfirm;
    private int _currentPage = 1;
    private int _totalCount;
    private Guid? _editingColorId;
    private Guid? _deletingColorId;
    private string? _statusMessage;
    private string? _errorMessage;

    [Inject]
    private IColorService ColorService { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ColorsResource> L { get; set; } = default!;

    [Inject]
    private ILogger<ColorsTab> Logger { get; set; } = default!;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_totalCount / _pageSize));

    private int PageStartIndex => _totalCount == 0 ? 0 : ((_currentPage - 1) * _pageSize) + 1;

    private int PageEndIndex => _totalCount == 0 ? 0 : Math.Min(_currentPage * _pageSize, _totalCount);

    private bool CanGoPreviousPage => _currentPage > 1;

    private bool CanGoNextPage => _currentPage < TotalPages;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await LoadColorsAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private async Task LoadColorsAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            ColorsPageDto page = await ColorService
                .GetPageAsync(_currentPage, _pageSize, _cts.Token)
                .ConfigureAwait(false);

            _colors.Clear();
            _colors.AddRange(page.Items);
            _totalCount = page.TotalCount;
        }
        catch (Exception exception)
        {
            LogLoadColorsFailed(Logger, exception);
            _errorMessage = L["Colors.Error.LoadFailed"];
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPreviousPage)
        {
            return;
        }

        _currentPage--;
        await LoadColorsAsync().ConfigureAwait(false);
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNextPage)
        {
            return;
        }

        _currentPage++;
        await LoadColorsAsync().ConfigureAwait(false);
    }

    private void OpenCreateModal()
    {
        _editingColorId = null;
        _form.Reset();
        _formErrors.Clear();
        _showModal = true;
        _statusMessage = null;
        _errorMessage = null;
    }

    private void OpenEditModal(ColorDto color)
    {
        _editingColorId = color.Id;
        _form.NameEn = color.NameEn;
        _form.NameFr = color.NameFr;
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

    private async Task SaveColorAsync()
    {
        _isSaving = true;
        _formErrors.Clear();

        try
        {
            SaveColorRequest request = new(_form.NameEn, _form.NameFr);

            if (_editingColorId is null)
            {
                Result<ColorDto> result = await ColorService.CreateAsync(request, _cts.Token).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    _formErrors.Add(L.LocalizeError(result.Error!));
                    _isSaving = false;
                    return;
                }

                _showModal = false;
                _statusMessage = L["Colors.Success.Created"];
            }
            else
            {
                Result<ColorDto> result = await ColorService
                    .UpdateAsync(_editingColorId.Value, request, _cts.Token)
                    .ConfigureAwait(false);

                if (result.IsFailure)
                {
                    _formErrors.Add(L.LocalizeError(result.Error!));
                    _isSaving = false;
                    return;
                }

                _showModal = false;
                _statusMessage = L["Colors.Success.Updated"];
            }

            await LoadColorsAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogSaveColorFailed(Logger, exception);
            _formErrors.Add(L["Colors.Error.UnexpectedError"]);
        }

        _isSaving = false;
    }

    private void ConfirmDelete(Guid colorId)
    {
        _deletingColorId = colorId;
        _showDeleteConfirm = true;
        _statusMessage = null;
        _errorMessage = null;
    }

    private void CancelDelete()
    {
        _showDeleteConfirm = false;
        _deletingColorId = null;
    }

    private async Task DeleteColorAsync()
    {
        if (_deletingColorId is null)
        {
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;

        try
        {
            Result result = await ColorService.DeleteAsync(_deletingColorId.Value, _cts.Token).ConfigureAwait(false);

            if (result.IsFailure)
            {
                _showDeleteConfirm = false;
                _deletingColorId = null;
                _errorMessage = L.LocalizeError(result.Error!);
                _isSaving = false;
                return;
            }

            _showDeleteConfirm = false;
            _deletingColorId = null;
            _statusMessage = L["Colors.Success.Deleted"];
            await LoadColorsAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogDeleteColorFailed(Logger, exception);
            _showDeleteConfirm = false;
            _deletingColorId = null;
            _errorMessage = L["Colors.Error.UnexpectedError"];
        }

        _isSaving = false;
    }

    private static string LocalizeName(string en, string fr)
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? fr : en;
    }

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to load colors")]
    private static partial void LogLoadColorsFailed(ILogger logger, Exception exception);

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to save color")]
    private static partial void LogSaveColorFailed(ILogger logger, Exception exception);

    [ExcludeFromCodeCoverage]
    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to delete color")]
    private static partial void LogDeleteColorFailed(ILogger logger, Exception exception);

    private sealed class ColorFormModel
    {
        public string NameEn { get; set; } = string.Empty;

        public string NameFr { get; set; } = string.Empty;

        public void Reset()
        {
            NameEn = string.Empty;
            NameFr = string.Empty;
        }
    }
}
