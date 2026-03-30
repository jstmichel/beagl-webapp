// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Setup.Dtos;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;

namespace Beagl.Application.Setup.Services;

/// <summary>
/// Implements first-run setup workflows.
/// </summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="setupStatusCache">The setup status cache.</param>
public sealed class InitialSetupService(
    IUserRepository userRepository,
    SetupStatusCache setupStatusCache) : IInitialSetupService
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly SetupStatusCache _setupStatusCache = setupStatusCache ?? throw new ArgumentNullException(nameof(setupStatusCache));

    /// <inheritdoc />
    public async Task<bool> IsSetupRequiredAsync(CancellationToken cancellationToken)
    {
        if (_setupStatusCache.IsSetupComplete)
        {
            return false;
        }

        bool hasAdministrator = await _userRepository.HasAnyAdministratorAsync(cancellationToken).ConfigureAwait(false);
        if (hasAdministrator)
        {
            _setupStatusCache.MarkSetupComplete();
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<Result> CompleteInitialSetupAsync(CompleteInitialSetupRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateRequest(request);
        if (validation.IsFailure)
        {
            return validation;
        }

        bool hasAdministrator = await _userRepository.HasAnyAdministratorAsync(cancellationToken).ConfigureAwait(false);
        if (hasAdministrator)
        {
            return Result.Failure(new ResultError("setup.already_completed", "Initial setup has already been completed."));
        }

        CreateUserAccount createAdministrator = new(
            request.UserName.Trim(),
            request.Email.Trim(),
            null,
            request.Password,
            UserRole.Administrator,
            EmailConfirmed: true);

        Result<UserAccount> createResult = await _userRepository
            .CreateAsync(createAdministrator, cancellationToken)
            .ConfigureAwait(false);

        if (createResult.IsFailure)
        {
            return Result.Failure(createResult.Error!);
        }

        _setupStatusCache.MarkSetupComplete();

        return Result.Success();
    }

    private static Result ValidateRequest(CompleteInitialSetupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure(new ResultError("setup.user_name_required", "A user name is required."));
        }

        if (request.UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
        {
            return Result.Failure(new ResultError("setup.user_name_too_long", "The user name must contain at most 256 characters."));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Failure(new ResultError("setup.email_required", "An email address is required."));
        }

        if (!EmailValidator.IsValid(request.Email.Trim()))
        {
            return Result.Failure(new ResultError("setup.invalid_email", "The email address is not valid."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure(new ResultError("setup.password_required", "A password is required."));
        }

        if (request.Password.Trim().Length < ValidationConstants.PasswordMinLength)
        {
            return Result.Failure(new ResultError("setup.password_too_short", "The password must contain at least 8 characters."));
        }

        return Result.Success();
    }
}
