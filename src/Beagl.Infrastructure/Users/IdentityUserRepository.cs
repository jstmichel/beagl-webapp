// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Users;

/// <summary>
/// Persists managed users through ASP.NET Core Identity.
/// </summary>
/// <param name="userManager">The identity user manager.</param>
public sealed class IdentityUserRepository(
    UserManager<ApplicationUser> userManager) : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserAccount>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.UserName)
            .Select(user => new UserAccount(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UserAccount?> GetByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _userManager.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new UserAccount(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<UserAccount>> CreateAsync(CreateUserAccount user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser identityUser = new()
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };

        IdentityResult createResult = await _userManager.CreateAsync(identityUser, user.Password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            return Result.Failure<UserAccount>(MapIdentityError(createResult));
        }

        return Result.Success(Map(identityUser));
    }

    /// <inheritdoc />
    public async Task<Result<UserAccount>> UpdateAsync(UpdateUserAccount user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? identityUser = await _userManager.FindByIdAsync(user.Id).ConfigureAwait(false);
        if (identityUser is null)
        {
            return Result.Failure<UserAccount>(new ResultError("users.not_found", "The requested user could not be found."));
        }

        identityUser.UserName = user.UserName;
        identityUser.Email = user.Email;
        identityUser.PhoneNumber = user.PhoneNumber;

        IdentityResult updateResult = await _userManager.UpdateAsync(identityUser).ConfigureAwait(false);
        if (!updateResult.Succeeded)
        {
            return Result.Failure<UserAccount>(MapIdentityError(updateResult));
        }

        return Result.Success(Map(identityUser));
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? identityUser = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (identityUser is null)
        {
            return Result.Failure(new ResultError("users.not_found", "The requested user could not be found."));
        }

        IdentityResult deleteResult = await _userManager.DeleteAsync(identityUser).ConfigureAwait(false);
        if (!deleteResult.Succeeded)
        {
            return Result.Failure(MapIdentityError(deleteResult));
        }

        return Result.Success();
    }

    private static UserAccount Map(ApplicationUser user)
    {
        return new UserAccount(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.PhoneNumber,
            user.EmailConfirmed,
            user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow);
    }

    private static ResultError MapIdentityError(IdentityResult identityResult)
    {
        IdentityError? identityError = identityResult.Errors.FirstOrDefault();
        if (identityError is null)
        {
            return new ResultError("users.identity_error", "The user operation could not be completed.");
        }

        return identityError.Code switch
        {
            "DuplicateUserName" => new ResultError("users.duplicate_user_name", "The user name is already in use."),
            "DuplicateEmail" => new ResultError("users.duplicate_email", "The email address is already in use."),
            "InvalidEmail" => new ResultError("users.invalid_email", "The email address is not valid."),
            "PasswordTooShort" => new ResultError("users.password_too_short", identityError.Description),
            _ => new ResultError("users.identity_error", identityError.Description),
        };
    }
}
