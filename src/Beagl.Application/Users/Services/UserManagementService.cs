// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Application.Users.Dtos;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Beagl.Application.Users.Services;

/// <summary>
/// Implements user management workflows.
/// </summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="logger">The logger.</param>
public sealed partial class UserManagementService(
    IUserRepository userRepository,
    ILogger<UserManagementService> logger) : IUserManagementService
{
    private static readonly EmailAddressAttribute _emailValidator = new();

    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ILogger<UserManagementService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<UsersMetricsDto> GetUsersMetricsAsync(CancellationToken cancellationToken)
    {
        UsersMetrics metrics = await _userRepository.GetMetricsAsync(cancellationToken).ConfigureAwait(false);

        return new UsersMetricsDto(metrics.TotalUsers, metrics.PendingConfirmationUsers, metrics.LockedOutUsers);
    }

    /// <inheritdoc />
    public async Task<UsersPageDto> GetUsersPageAsync(GetUsersPageRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        int pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        int pageSize = request.PageSize switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => request.PageSize,
        };

        GetUsersPageQuery query = new(
            string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim(),
            pageNumber,
            pageSize);

        UsersPage page = await _userRepository.GetPageAsync(query, cancellationToken).ConfigureAwait(false);

        return new UsersPageDto(
            [..page.Users.Select(MapListItem)],
            page.TotalCount,
            page.PageNumber,
            page.PageSize);
    }

    /// <inheritdoc />
    public async Task<Result<UserDetailsDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure<UserDetailsDto>(new ResultError("users.invalid_id", "A user identifier is required."));
        }

        UserAccount? user = await _userRepository.GetByIdAsync(userId.Trim(), cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Result.Failure<UserDetailsDto>(new ResultError("users.not_found", "The requested user could not be found."));
        }

        return Result.Success(MapDetails(user));
    }

    /// <inheritdoc />
    public async Task<Result<UserDetailsDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateCreateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<UserDetailsDto>(validation.Error!);
        }

        CreateUserAccount createUser = new(
            request.UserName.Trim(),
            NormalizeOptionalValue(request.Email),
            NormalizeOptionalValue(request.PhoneNumber),
            request.Password,
            request.Role);

        Result<UserAccount> result = await _userRepository.CreateAsync(createUser, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result.Failure<UserDetailsDto>(result.Error!);
        }

        LogUserCreated(_logger, result.Value!.Id);
        return Result.Success(MapDetails(result.Value));
    }

    /// <inheritdoc />
    public async Task<Result<UserDetailsDto>> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateUpdateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<UserDetailsDto>(validation.Error!);
        }

        UpdateUserAccount updateUser = new(
            request.Id.Trim(),
            request.UserName.Trim(),
            NormalizeOptionalValue(request.Email),
            NormalizeOptionalValue(request.PhoneNumber),
            request.Role);

        Result<UserAccount> result = await _userRepository.UpdateAsync(updateUser, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result.Failure<UserDetailsDto>(result.Error!);
        }

        LogUserUpdated(_logger, result.Value!.Id);
        return Result.Success(MapDetails(result.Value));
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure(new ResultError("users.invalid_id", "A user identifier is required."));
        }

        string trimmedUserId = userId.Trim();
        Result result = await _userRepository.DeleteAsync(trimmedUserId, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            LogUserDeleted(_logger, trimmedUserId);
        }

        return result;
    }

    private static UserListItemDto MapListItem(UserAccount user)
    {
        return new UserListItemDto(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.EmailConfirmed,
            user.IsLockedOut,
            user.Role);
    }

    private static UserDetailsDto MapDetails(UserAccount user)
    {
        return new UserDetailsDto(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.EmailConfirmed,
            user.IsLockedOut,
            user.Role);
    }

    private static Result ValidateCreateRequest(CreateUserRequest request)
    {
        if (!Enum.IsDefined(request.Role) || request.Role == UserRole.None)
        {
            return Result.Failure(new ResultError("users.invalid_role", "The specified user role is not valid."));
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure(new ResultError("users.user_name_required", "A user name is required."));
        }

        if (request.UserName.Trim().Length > 256)
        {
            return Result.Failure(new ResultError("users.user_name_too_long", "The user name must contain at most 256 characters."));
        }

        if (request.Role is UserRole.Employee or UserRole.Administrator
            && string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Failure(new ResultError("users.email_required", "An email address is required."));
        }

        if (!string.IsNullOrWhiteSpace(request.Email)
            && !_emailValidator.IsValid(request.Email.Trim()))
        {
            return Result.Failure(new ResultError("users.invalid_email", "The email address is not valid."));
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return Result.Failure(new ResultError("users.phone_required", "A phone number is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure(new ResultError("users.password_required", "A password is required."));
        }

        if (request.Password.Trim().Length < 8)
        {
            return Result.Failure(new ResultError("users.password_too_short", "The password must contain at least 8 characters."));
        }

        return Result.Success();
    }

    private static Result ValidateUpdateRequest(UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return Result.Failure(new ResultError("users.invalid_id", "A user identifier is required."));
        }

        if (!Enum.IsDefined(request.Role) || request.Role == UserRole.None)
        {
            return Result.Failure(new ResultError("users.invalid_role", "The specified user role is not valid."));
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure(new ResultError("users.user_name_required", "A user name is required."));
        }

        if (request.UserName.Trim().Length > 256)
        {
            return Result.Failure(new ResultError("users.user_name_too_long", "The user name must contain at most 256 characters."));
        }

        if (request.Role is UserRole.Employee or UserRole.Administrator
            && string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Failure(new ResultError("users.email_required", "An email address is required."));
        }

        if (!string.IsNullOrWhiteSpace(request.Email)
            && !_emailValidator.IsValid(request.Email.Trim()))
        {
            return Result.Failure(new ResultError("users.invalid_email", "The email address is not valid."));
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return Result.Failure(new ResultError("users.phone_required", "A phone number is required."));
        }

        return Result.Success();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Created managed user {UserId}")]
    private static partial void LogUserCreated(ILogger logger, string userId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Updated managed user {UserId}")]
    private static partial void LogUserUpdated(ILogger logger, string userId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Deleted managed user {UserId}")]
    private static partial void LogUserDeleted(ILogger logger, string userId);
}
