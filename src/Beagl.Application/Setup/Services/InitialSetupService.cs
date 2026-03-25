// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Application.Setup.Dtos;
using Beagl.Domain.Results;
using Beagl.Domain.Users;

namespace Beagl.Application.Setup.Services;

/// <summary>
/// Implements first-run setup workflows.
/// </summary>
/// <param name="userRepository">The user repository.</param>
public sealed class InitialSetupService(IUserRepository userRepository) : IInitialSetupService
{
    private static readonly EmailAddressAttribute _emailValidator = new();
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    /// <inheritdoc />
    public async Task<bool> IsSetupRequiredAsync(CancellationToken cancellationToken)
    {
        bool hasAdministrator = await _userRepository.HasAnyAdministratorAsync(cancellationToken).ConfigureAwait(false);
        return !hasAdministrator;
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

        return Result.Success();
    }

    private static Result ValidateRequest(CompleteInitialSetupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure(new ResultError("setup.user_name_required", "A user name is required."));
        }

        if (request.UserName.Trim().Length > 256)
        {
            return Result.Failure(new ResultError("setup.user_name_too_long", "The user name must contain at most 256 characters."));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Failure(new ResultError("setup.email_required", "An email address is required."));
        }

        if (!_emailValidator.IsValid(request.Email.Trim()))
        {
            return Result.Failure(new ResultError("setup.invalid_email", "The email address is not valid."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure(new ResultError("setup.password_required", "A password is required."));
        }

        if (request.Password.Trim().Length < 8)
        {
            return Result.Failure(new ResultError("setup.password_too_short", "The password must contain at least 8 characters."));
        }

        return Result.Success();
    }
}
