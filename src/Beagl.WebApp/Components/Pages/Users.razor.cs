// MIT License - Copyright (c) 2025 Jonathan St-Michel

// Blazor event handlers catch all exceptions to prevent SignalR circuit failures.
#pragma warning disable CA1031 // Do not catch general exception types

using System.ComponentModel.DataAnnotations;
using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.WebApp.Extensions;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Beagl.WebApp.Components.Pages;

/// <summary>
/// Code-behind for the user management page.
/// </summary>
public sealed partial class Users : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<UserListItemDto> _users = [];
    private readonly CreateUserFormModel _createForm = new();
    private readonly EditUserFormModel _editForm = new();
    private const int _pageSize = 10;

    private bool _isLoading = true;
    private bool _isSaving;
    private string _searchText = string.Empty;
    private int _currentPage = 1;
    private int _filteredUsersCount;
    private int _totalUsersCount;
    private int _pendingConfirmationUsersCount;
    private int _lockedOutUsersCount;
    private string? _errorMessage;
    private string? _statusMessage;
    private string? _confirmationUrl;
    private UserPanelMode _panelMode;
    private UserDetailsDto? _selectedUser;
    private EditContext _createEditContext = default!;
    private EditContext _editEditContext = default!;

    [Inject]
    private IUserManagementService UserManagementService { get; set; } = default!;

    [Inject]
    private ILogger<Users> Logger { get; set; } = default!;

    [Inject]
    private IStringLocalizer<UsersResource> L { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_filteredUsersCount / _pageSize));

    private int PageStartIndex => _filteredUsersCount == 0 ? 0 : ((_currentPage - 1) * _pageSize) + 1;

    private int PageEndIndex => _filteredUsersCount == 0 ? 0 : Math.Min(_currentPage * _pageSize, _filteredUsersCount);

    private bool CanGoPreviousPage => _currentPage > 1;

    private bool CanGoNextPage => _currentPage < TotalPages;

    private string PanelTitle => _panelMode switch
    {
        UserPanelMode.Create => L["Users.Panel.Title.Create"],
        UserPanelMode.Details => L["Users.Panel.Title.Details"],
        UserPanelMode.Edit => L["Users.Panel.Title.Edit"],
        UserPanelMode.Delete => L["Users.Panel.Title.Delete"],
        _ => string.Empty,
    };

    private string PanelDescription => _panelMode switch
    {
        UserPanelMode.Create => L["Users.Panel.Description.Create"],
        UserPanelMode.Details => L["Users.Panel.Description.Details"],
        UserPanelMode.Edit => L["Users.Panel.Description.Edit"],
        UserPanelMode.Delete => L["Users.Panel.Description.Delete"],
        _ => string.Empty,
    };

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        _createEditContext = new(_createForm);
        _editEditContext = new(_editForm);
        await LoadUsersMetricsAsync().ConfigureAwait(false);
        await LoadUsersPageAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private async Task LoadUsersMetricsAsync()
    {
        try
        {
            UsersMetricsDto metrics = await UserManagementService.GetUsersMetricsAsync(_cts.Token).ConfigureAwait(false);
            _totalUsersCount = metrics.TotalUsers;
            _pendingConfirmationUsersCount = metrics.PendingConfirmationUsers;
            _lockedOutUsersCount = metrics.LockedOutUsers;
        }
        catch (Exception exception)
        {
            LogLoadUsersMetricsFailed(Logger, exception);
            _errorMessage ??= L["Users.Error.LoadFailed"];
        }
    }

    private async Task LoadUsersPageAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            GetUsersPageRequest request = new(_searchText, _currentPage, _pageSize);
            UsersPageDto page = await UserManagementService.GetUsersPageAsync(request, _cts.Token).ConfigureAwait(false);

            _users.Clear();
            _users.AddRange(page.Users);
            _filteredUsersCount = page.TotalCount;
            _currentPage = page.PageNumber;
        }
        catch (Exception exception)
        {
            LogLoadUsersFailed(Logger, exception);
            _errorMessage = L["Users.Error.LoadFailed"];
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OpenCreatePanel()
    {
        _createForm.Reset();
        _createEditContext = new(_createForm);
        _selectedUser = null;
        _panelMode = UserPanelMode.Create;
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;
    }

    private async Task OpenDetailsPanelAsync(string userId)
    {
        await LoadSelectedUserAsync(userId, UserPanelMode.Details).ConfigureAwait(false);
    }

    private async Task OpenEditPanelAsync(string userId)
    {
        bool isLoaded = await LoadSelectedUserAsync(userId, UserPanelMode.Edit).ConfigureAwait(false);
        if (isLoaded && _selectedUser is not null)
        {
            _editForm.Load(_selectedUser);
            _editEditContext = new(_editForm);
        }
    }

    private async Task OpenDeletePanelAsync(string userId)
    {
        await LoadSelectedUserAsync(userId, UserPanelMode.Delete).ConfigureAwait(false);
    }

    private void ClosePanel()
    {
        _panelMode = UserPanelMode.None;
        _selectedUser = null;
        _errorMessage = null;
    }

    private async Task CreateUserAsync()
    {
        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;

        try
        {
            CreateUserRequest request = new(
                _createForm.UserName,
                _createForm.Email,
                _createForm.PhoneNumber,
                _createForm.Password,
                _createForm.Role);

            Result<UserDetailsDto> result = await UserManagementService.CreateAsync(request, _cts.Token).ConfigureAwait(false);
            await HandleMutationResultAsync(result, L["Users.Success.Created"], UserPanelMode.Details).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogCreateUserFailed(Logger, exception);
            _errorMessage = L["Users.Error.UnexpectedError"];
            _isSaving = false;
        }
    }

    private async Task UpdateUserAsync()
    {
        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;

        try
        {
            UpdateUserRequest request = new(
                _editForm.Id,
                _editForm.UserName,
                _editForm.Email,
                _editForm.PhoneNumber,
                _editForm.Role);

            Result<UserDetailsDto> result = await UserManagementService.UpdateAsync(request, _cts.Token).ConfigureAwait(false);
            await HandleMutationResultAsync(result, L["Users.Success.Updated"], UserPanelMode.Details).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogUpdateUserFailed(Logger, exception);
            _errorMessage = L["Users.Error.UnexpectedError"];
            _isSaving = false;
        }
    }

    private async Task DeleteUserAsync()
    {
        if (_selectedUser is null)
        {
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;

        Result result = await UserManagementService.DeleteAsync(_selectedUser.Id, _cts.Token).ConfigureAwait(false);
        if (result.IsFailure)
        {
            _errorMessage = L.LocalizeError(result.Error!);
            _isSaving = false;
            return;
        }

        _selectedUser = null;
        _panelMode = UserPanelMode.None;
        _statusMessage = L["Users.Success.Deleted"];
        await LoadUsersMetricsAsync().ConfigureAwait(false);
        await LoadUsersPageAsync().ConfigureAwait(false);
        _isSaving = false;
    }

    private async Task ConfirmUserAccountAsync(string userId)
    {
        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;

        Result<UserDetailsDto> result = await UserManagementService
            .ConfirmAccountAsync(userId, _cts.Token)
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            _errorMessage = L.LocalizeError(result.Error!);
            _isSaving = false;
            return;
        }

        if (_selectedUser is not null && _selectedUser.Id == result.Value!.Id)
        {
            _selectedUser = result.Value;
        }

        _statusMessage = L["Users.Success.AccountConfirmed"];
        await LoadUsersMetricsAsync().ConfigureAwait(false);
        await LoadUsersPageAsync().ConfigureAwait(false);
        _isSaving = false;
    }

    private async Task ConfirmSelectedUserAccountAsync()
    {
        if (_selectedUser is null)
        {
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;

        Result<UserDetailsDto> result = await UserManagementService
            .ConfirmAccountAsync(_selectedUser.Id, _cts.Token)
            .ConfigureAwait(false);
        await HandleMutationResultAsync(result, L["Users.Success.AccountConfirmed"], UserPanelMode.Details).ConfigureAwait(false);
    }

    private async Task CopyConfirmationLinkAsync()
    {
        if (_selectedUser is null || _selectedUser.EmailConfirmed)
        {
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;

        Result<string> result = await UserManagementService
            .GenerateEmailConfirmationTokenAsync(_selectedUser.Id, _cts.Token)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            _panelMode = UserPanelMode.None;
            _errorMessage = L.LocalizeError(result.Error!);
            _isSaving = false;
            return;
        }

        string confirmationUrl = NavigationManager.ToAbsoluteUri(
            $"/account/confirm-email?userId={Uri.EscapeDataString(_selectedUser.Id)}&token={Uri.EscapeDataString(result.Value!)}").ToString();

        _confirmationUrl = null;

        try
        {
            bool copied = await JS.InvokeAsync<bool>("beagl.copyToClipboard", confirmationUrl).ConfigureAwait(false);

            if (copied)
            {
                _statusMessage = L["Users.Success.ConfirmationLinkCopied"];
            }
            else
            {
                _confirmationUrl = confirmationUrl;
            }
        }
        catch (Exception exception)
        {
            LogClipboardWriteFailed(Logger, exception);
            _confirmationUrl = confirmationUrl;
        }

        _panelMode = UserPanelMode.None;
        _isSaving = false;
    }

    private async Task<bool> LoadSelectedUserAsync(string userId, UserPanelMode panelMode)
    {
        _errorMessage = null;
        _statusMessage = null;
        _confirmationUrl = null;

        Result<UserDetailsDto> result = await UserManagementService.GetUserByIdAsync(userId, _cts.Token).ConfigureAwait(false);
        if (result.IsFailure)
        {
            _errorMessage = L.LocalizeError(result.Error!);
            return false;
        }

        _selectedUser = result.Value!;
        _panelMode = panelMode;
        return true;
    }

    private async Task HandleMutationResultAsync(Result<UserDetailsDto> result, string successMessage, UserPanelMode successPanelMode)
    {
        if (result.IsFailure)
        {
            _errorMessage = L.LocalizeError(result.Error!);
            _isSaving = false;
            return;
        }

        _selectedUser = result.Value!;
        _panelMode = successPanelMode;
        _statusMessage = successMessage;
        await LoadUsersMetricsAsync().ConfigureAwait(false);
        await LoadUsersPageAsync().ConfigureAwait(false);
        _isSaving = false;
    }

    private string FormatOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? L["Users.Optional.NotProvided"] : value;
    }

    private static IReadOnlyList<UserRole> SelectableRoles =>
    [
        UserRole.Citizen,
        UserRole.Employee,
        UserRole.Administrator,
    ];

    private string LocalizeRole(UserRole role)
    {
        LocalizedString localizedRole = L[$"Users.Role.{role}"];
        return localizedRole.ResourceNotFound ? role.ToString() : localizedRole.Value;
    }

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        _searchText = args.Value?.ToString() ?? string.Empty;
        _currentPage = 1;
        await LoadUsersPageAsync().ConfigureAwait(false);
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPreviousPage)
        {
            return;
        }

        _currentPage--;
        await LoadUsersPageAsync().ConfigureAwait(false);
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNextPage)
        {
            return;
        }

        _currentPage++;
        await LoadUsersPageAsync().ConfigureAwait(false);
    }

    private static string GetAvatarInitial(UserListItemDto user)
    {
        return string.IsNullOrWhiteSpace(user.UserName)
            ? "?"
            : user.UserName[..1].ToUpperInvariant();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to load users metrics")]
    private static partial void LogLoadUsersMetricsFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to load managed users")]
    private static partial void LogLoadUsersFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create user")]
    private static partial void LogCreateUserFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to update user")]
    private static partial void LogUpdateUserFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Clipboard write failed")]
    private static partial void LogClipboardWriteFailed(ILogger logger, Exception exception);

    private enum UserPanelMode
    {
        None,
        Create,
        Details,
        Edit,
        Delete,
    }

    private sealed class CreateUserFormModel : IValidatableObject
    {
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Employee;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                yield return new ValidationResult("users.user_name_required", [nameof(UserName)]);
            }
            else if (UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
            {
                yield return new ValidationResult("users.user_name_too_long", [nameof(UserName)]);
            }

            if (Role is UserRole.Employee or UserRole.Administrator && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("users.email_required", [nameof(Email)]);
            }
            else if (!string.IsNullOrWhiteSpace(Email) && !EmailValidator.IsValid(Email.Trim()))
            {
                yield return new ValidationResult("users.invalid_email", [nameof(Email)]);
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult("users.phone_required", [nameof(PhoneNumber)]);
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("users.password_required", [nameof(Password)]);
            }
            else if (Password.Length < ValidationConstants.PasswordMinLength)
            {
                yield return new ValidationResult("users.password_too_short", [nameof(Password)]);
            }
        }

        public void Reset()
        {
            UserName = string.Empty;
            Email = null;
            PhoneNumber = null;
            Password = string.Empty;
            Role = UserRole.Employee;
        }
    }

    private sealed class EditUserFormModel : IValidatableObject
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; } = UserRole.Employee;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                yield return new ValidationResult("users.user_name_required", [nameof(UserName)]);
            }
            else if (UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
            {
                yield return new ValidationResult("users.user_name_too_long", [nameof(UserName)]);
            }

            if (Role is UserRole.Employee or UserRole.Administrator && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("users.email_required", [nameof(Email)]);
            }
            else if (!string.IsNullOrWhiteSpace(Email) && !EmailValidator.IsValid(Email.Trim()))
            {
                yield return new ValidationResult("users.invalid_email", [nameof(Email)]);
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult("users.phone_required", [nameof(PhoneNumber)]);
            }
        }

        public void Load(UserDetailsDto user)
        {
            Id = user.Id;
            UserName = user.UserName;
            Email = string.IsNullOrEmpty(user.Email) ? null : user.Email;
            PhoneNumber = user.PhoneNumber;
            Role = user.Role;
        }
    }
}
