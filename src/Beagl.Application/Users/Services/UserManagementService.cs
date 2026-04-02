// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Dtos;
using Beagl.Domain;
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

    /// <inheritdoc />
    public async Task<Result<UserDetailsDto>> ConfirmAccountAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure<UserDetailsDto>(new ResultError("users.invalid_id", "A user identifier is required."));
        }

        string trimmedUserId = userId.Trim();
        Result<UserAccount> result = await _userRepository.ConfirmAccountAsync(trimmedUserId, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result.Failure<UserDetailsDto>(result.Error!);
        }

        LogUserConfirmed(_logger, trimmedUserId);
        return Result.Success(MapDetails(result.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure<string>(new ResultError("users.invalid_id", "A user identifier is required."));
        }

        string trimmedUserId = userId.Trim();
        Result<string> result = await _userRepository
            .GenerateEmailConfirmationTokenAsync(trimmedUserId, cancellationToken)
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        LogTokenGenerated(_logger, trimmedUserId);
        return result;
    }

    /// <inheritdoc />
    public async Task<Result> ConfirmAccountByTokenAsync(string userId, string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure(new ResultError("users.invalid_id", "A user identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure(new ResultError("users.token_required", "An email confirmation token is required."));
        }

        string trimmedUserId = userId.Trim();
        Result result = await _userRepository
            .ConfirmAccountByTokenAsync(trimmedUserId, token, cancellationToken)
            .ConfigureAwait(false);
        if (result.IsSuccess)
        {
            LogUserConfirmedByToken(_logger, trimmedUserId);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Result<UserDetailsDto>> RegisterCitizenAsync(RegisterCitizenRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateRegisterCitizenRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<UserDetailsDto>(validation.Error!);
        }

        RegisterCitizenAccount account = new(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.UserName.Trim(),
            request.PhoneNumber.Trim(),
            NormalizeOptionalValue(request.Email),
            request.Password);

        Result<UserAccount> result = await _userRepository.RegisterCitizenAsync(account, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result.Failure<UserDetailsDto>(result.Error!);
        }

        LogCitizenRegistered(_logger, result.Value!.Id);
        return Result.Success(MapDetails(result.Value));
    }

    /// <inheritdoc />
    public async Task<Result> RequestRecoveryCodeAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Result.Success();
        }

        string trimmedIdentifier = identifier.Trim();

        UserAccount? user = await _userRepository
            .FindByIdentifierAsync(trimmedIdentifier, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result.Success();
        }

        Result result = await _userRepository
            .GenerateRecoveryCodeAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            LogRecoveryCodeRequested(_logger, trimmedIdentifier);
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RecoverAccountAsync(RecoverAccountRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Result.Failure(new ResultError("users.recovery_code_required", "A recovery code is required."));
        }

        if (request.Code.Trim().Length != ValidationConstants.RecoveryCodeLength)
        {
            return Result.Failure(new ResultError("users.invalid_recovery_code", "The recovery code is invalid."));
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Result.Failure(new ResultError("users.password_required", "A password is required."));
        }

        if (request.NewPassword.Trim().Length < ValidationConstants.PasswordMinLength)
        {
            return Result.Failure(new ResultError("users.password_too_short", "The password must contain at least 8 characters."));
        }

        Result result = await _userRepository
            .ResetPasswordByRecoveryCodeAsync(request.Code.Trim().ToUpperInvariant(), request.NewPassword, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            LogAccountRecovered(_logger);
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
            user.Role,
            user.RecoveryCode is not null);
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
            user.Role,
            user.EmailConfirmationToken,
            user.RecoveryCode);
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

        if (request.UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
        {
            return Result.Failure(new ResultError("users.user_name_too_long", "The user name must contain at most 256 characters."));
        }

        if (request.Role is UserRole.Employee or UserRole.Administrator
            && string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Failure(new ResultError("users.email_required", "An email address is required."));
        }

        if (!string.IsNullOrWhiteSpace(request.Email)
            && !EmailValidator.IsValid(request.Email.Trim()))
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

        if (request.Password.Trim().Length < ValidationConstants.PasswordMinLength)
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

        if (request.UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
        {
            return Result.Failure(new ResultError("users.user_name_too_long", "The user name must contain at most 256 characters."));
        }

        if (request.Role is UserRole.Employee or UserRole.Administrator
            && string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Failure(new ResultError("users.email_required", "An email address is required."));
        }

        if (!string.IsNullOrWhiteSpace(request.Email)
            && !EmailValidator.IsValid(request.Email.Trim()))
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

    private static Result ValidateRegisterCitizenRequest(RegisterCitizenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return Result.Failure(new ResultError("users.first_name_required", "A first name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result.Failure(new ResultError("users.last_name_required", "A last name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure(new ResultError("users.user_name_required", "A user name is required."));
        }

        if (request.UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
        {
            return Result.Failure(new ResultError("users.user_name_too_long", "The user name must contain at most 256 characters."));
        }

        if (!string.IsNullOrWhiteSpace(request.Email)
            && !EmailValidator.IsValid(request.Email.Trim()))
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

        if (request.Password.Trim().Length < ValidationConstants.PasswordMinLength)
        {
            return Result.Failure(new ResultError("users.password_too_short", "The password must contain at least 8 characters."));
        }

        return Result.Success();
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Created managed user {UserId}")]
    private static partial void LogUserCreated(ILogger logger, string userId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Updated managed user {UserId}")]
    private static partial void LogUserUpdated(ILogger logger, string userId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Deleted managed user {UserId}")]
    private static partial void LogUserDeleted(ILogger logger, string userId);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Confirmed managed user account {UserId}")]
    private static partial void LogUserConfirmed(ILogger logger, string userId);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Information, Message = "Generated email confirmation token for managed user {UserId}")]
    private static partial void LogTokenGenerated(ILogger logger, string userId);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Information, Message = "Confirmed managed user account {UserId} by token")]
    private static partial void LogUserConfirmedByToken(ILogger logger, string userId);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Information, Message = "Citizen self-registered user {UserId}")]
    private static partial void LogCitizenRegistered(ILogger logger, string userId);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Information, Message = "Recovery code requested for identifier {Identifier}")]
    private static partial void LogRecoveryCodeRequested(ILogger logger, string identifier);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Information, Message = "Account recovered by recovery code")]
    private static partial void LogAccountRecovered(ILogger logger);
}
