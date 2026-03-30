// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.Infrastructure.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Users;

/// <summary>
/// Persists managed users through ASP.NET Core Identity.
/// </summary>
/// <param name="userManager">The identity user manager.</param>
/// <param name="applicationDbContext">The application database context.</param>
public sealed class IdentityUserRepository(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext applicationDbContext) : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));

    /// <inheritdoc />
    public async Task<bool> HasAnyAdministratorAsync(CancellationToken cancellationToken)
    {
        string administratorRoleName = UserRole.Administrator.ToString();

        return await (
                from userRole in _applicationDbContext.Set<IdentityUserRole<string>>().AsNoTracking()
                join role in _applicationDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where role.Name == administratorRoleName
                select userRole.UserId)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UsersMetrics> GetMetricsAsync(CancellationToken cancellationToken)
    {
        IQueryable<ApplicationUser> usersQuery = _userManager.Users.AsNoTracking();

        int totalUsers = await usersQuery.CountAsync(cancellationToken).ConfigureAwait(false);
        int pendingConfirmationUsers = await usersQuery
            .CountAsync(user => !user.EmailConfirmed, cancellationToken)
            .ConfigureAwait(false);
        int lockedOutUsers = await usersQuery
            .CountAsync(user => user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        return new UsersMetrics(totalUsers, pendingConfirmationUsers, lockedOutUsers);
    }

    /// <inheritdoc />
    public async Task<UsersPage> GetPageAsync(GetUsersPageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<ApplicationUser> usersQuery = _userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string pattern = $"%{query.SearchTerm.Trim()}%";
            usersQuery = usersQuery.Where(user =>
                EF.Functions.ILike(user.UserName ?? string.Empty, pattern)
                || EF.Functions.ILike(user.Email ?? string.Empty, pattern));
        }

        int totalCount = await usersQuery.CountAsync(cancellationToken).ConfigureAwait(false);
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / query.PageSize));
        int currentPage = Math.Min(query.PageNumber, totalPages);

        List<ApplicationUser> identityUsers = await usersQuery
            .OrderBy(user => user.UserName)
            .ThenBy(user => user.Email)
            .Skip((currentPage - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, UserRole> rolesByUserId = await GetPrimaryRolesByUserIdAsync(
                identityUsers.Select(user => user.Id),
                cancellationToken)
            .ConfigureAwait(false);

        List<UserAccount> users = new(identityUsers.Count);
        foreach (ApplicationUser identityUser in identityUsers)
        {
            UserRole role = rolesByUserId.GetValueOrDefault(identityUser.Id, UserRole.None);
            users.Add(MapUser(identityUser, role));
        }

        return new UsersPage(users, totalCount, currentPage, query.PageSize);
    }

    /// <inheritdoc />
    public async Task<UserAccount?> GetByIdAsync(string userId, CancellationToken cancellationToken)
    {
        ApplicationUser? identityUser = await _userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken)
            .ConfigureAwait(false);
        if (identityUser is null)
        {
            return null;
        }

        UserRole role = await GetPrimaryRoleAsync(identityUser).ConfigureAwait(false);

        return new UserAccount(
            identityUser.Id,
            identityUser.UserName ?? string.Empty,
            identityUser.Email ?? string.Empty,
            identityUser.PhoneNumber,
            identityUser.EmailConfirmed,
            identityUser.LockoutEnd.HasValue && identityUser.LockoutEnd.Value > DateTimeOffset.UtcNow,
            role);
    }

    /// <inheritdoc />
    public async Task<Result<UserAccount>> CreateAsync(CreateUserAccount user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        if (user.Email is not null)
        {
            string normalizedEmail = user.Email.ToUpperInvariant();
            bool emailExists = await EmailExistsAsync(
                    normalizedEmail,
                    excludedUserId: null,
                    cancellationToken)
                .ConfigureAwait(false);
            if (emailExists)
            {
                return Result.Failure<UserAccount>(new ResultError("users.duplicate_email", "The email address is already in use."));
            }
        }

        ApplicationUser identityUser = new()
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
        };

        IdentityResult createResult = await _userManager.CreateAsync(identityUser, user.Password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            return Result.Failure<UserAccount>(MapIdentityError(createResult));
        }

        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(identityUser, user.Role.ToString()).ConfigureAwait(false);
        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(identityUser).ConfigureAwait(false);
            return Result.Failure<UserAccount>(MapIdentityError(addToRoleResult));
        }

        string? emailConfirmationToken = await GenerateEmailConfirmationTokenAsync(identityUser).ConfigureAwait(false);
        UserAccount createdUser = await MapAsync(identityUser).ConfigureAwait(false);

        return Result.Success(createdUser with { EmailConfirmationToken = emailConfirmationToken });
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

        if (user.Email is not null)
        {
            string normalizedEmail = user.Email.ToUpperInvariant();
            bool emailExists = await EmailExistsAsync(
                    normalizedEmail,
                    identityUser.Id,
                    cancellationToken)
                .ConfigureAwait(false);
            if (emailExists)
            {
                return Result.Failure<UserAccount>(new ResultError("users.duplicate_email", "The email address is already in use."));
            }
        }

        IdentityResult updateResult = await _userManager.UpdateAsync(identityUser).ConfigureAwait(false);
        if (!updateResult.Succeeded)
        {
            return Result.Failure<UserAccount>(MapIdentityError(updateResult));
        }

        IdentityResult roleUpdateResult = await ReplaceRolesAsync(identityUser, user.Role).ConfigureAwait(false);
        if (!roleUpdateResult.Succeeded)
        {
            return Result.Failure<UserAccount>(MapIdentityError(roleUpdateResult));
        }

        return Result.Success(await MapAsync(identityUser).ConfigureAwait(false));
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

    /// <inheritdoc />
    public async Task<Result<UserAccount>> ConfirmAccountAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? identityUser = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (identityUser is null)
        {
            return Result.Failure<UserAccount>(new ResultError("users.not_found", "The requested user could not be found."));
        }

        if (!identityUser.EmailConfirmed)
        {
            identityUser.EmailConfirmed = true;

            IdentityResult updateResult = await _userManager.UpdateAsync(identityUser).ConfigureAwait(false);
            if (!updateResult.Succeeded)
            {
                return Result.Failure<UserAccount>(MapIdentityError(updateResult));
            }
        }

        return Result.Success(await MapAsync(identityUser).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? identityUser = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (identityUser is null)
        {
            return Result.Failure<string>(new ResultError("users.not_found", "The requested user could not be found."));
        }

        if (identityUser.EmailConfirmed)
        {
            return Result.Failure<string>(new ResultError("users.already_confirmed", "The account is already confirmed."));
        }

        if (string.IsNullOrWhiteSpace(identityUser.Email))
        {
            return Result.Failure<string>(new ResultError("users.email_required", "An email address is required."));
        }

        string token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser).ConfigureAwait(false);

        return Result.Success(token);
    }

    /// <inheritdoc />
    public async Task<Result> ConfirmAccountByTokenAsync(string userId, string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? identityUser = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (identityUser is null)
        {
            return Result.Failure(new ResultError("users.not_found", "The requested user could not be found."));
        }

        IdentityResult confirmResult = await _userManager.ConfirmEmailAsync(identityUser, token).ConfigureAwait(false);
        if (!confirmResult.Succeeded)
        {
            return Result.Failure(new ResultError("users.invalid_token", "The confirmation token is invalid or has expired."));
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<UserAccount>> RegisterCitizenAsync(RegisterCitizenAccount account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser identityUser = new()
        {
            UserName = account.UserName,
            Email = account.Email,
            PhoneNumber = account.PhoneNumber,
            EmailConfirmed = false,
        };

        if (account.Email is not null)
        {
            string normalizedEmail = account.Email.ToUpperInvariant();
            bool emailExists = await EmailExistsAsync(
                    normalizedEmail,
                    excludedUserId: null,
                    CancellationToken.None)
                .ConfigureAwait(false);
            if (emailExists)
            {
                return Result.Failure<UserAccount>(new ResultError("users.duplicate_email", "The email address is already in use."));
            }
        }

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _applicationDbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        IdentityResult createResult = await _userManager.CreateAsync(identityUser, account.Password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result.Failure<UserAccount>(MapIdentityError(createResult));
        }

        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(identityUser, UserRole.Citizen.ToString()).ConfigureAwait(false);
        if (!addToRoleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result.Failure<UserAccount>(MapIdentityError(addToRoleResult));
        }

        CitizenProfileEntity profile = new()
        {
            Id = Guid.NewGuid(),
            UserId = identityUser.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
        };

        _applicationDbContext.CitizenProfiles.Add(profile);
        await _applicationDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        string? emailConfirmationToken = await GenerateEmailConfirmationTokenAsync(identityUser).ConfigureAwait(false);
        UserAccount createdUser = await MapAsync(identityUser).ConfigureAwait(false);

        return Result.Success(createdUser with { EmailConfirmationToken = emailConfirmationToken });
    }

    private async Task<UserAccount> MapAsync(ApplicationUser user)
    {
        UserRole role = await GetPrimaryRoleAsync(user).ConfigureAwait(false);

        return MapUser(user, role);
    }

    private async Task<Dictionary<string, UserRole>> GetPrimaryRolesByUserIdAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken)
    {
        List<string> distinctUserIds = [.. userIds.Where(static userId => !string.IsNullOrWhiteSpace(userId)).Distinct()];
        if (distinctUserIds.Count == 0)
        {
            return [];
        }

        List<UserRoleAssignment> roleAssignments = await (
            from userRole in _applicationDbContext.Set<IdentityUserRole<string>>().AsNoTracking()
            join role in _applicationDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where distinctUserIds.Contains(userRole.UserId)
            orderby userRole.UserId, role.Name
            select new UserRoleAssignment(userRole.UserId, role.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, UserRole> rolesByUserId = [];
        foreach (UserRoleAssignment roleAssignment in roleAssignments)
        {
            if (rolesByUserId.ContainsKey(roleAssignment.UserId))
            {
                continue;
            }

            if (Enum.TryParse<UserRole>(roleAssignment.RoleName, out UserRole role))
            {
                rolesByUserId[roleAssignment.UserId] = role;
            }
        }

        return rolesByUserId;
    }

    private static UserAccount MapUser(ApplicationUser user, UserRole role)
    {

        return new UserAccount(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.PhoneNumber,
            user.EmailConfirmed,
            user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow,
            role);
    }

    private async Task<UserRole> GetPrimaryRoleAsync(ApplicationUser user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        string? roleName = roles.FirstOrDefault();

        return Enum.TryParse<UserRole>(roleName, out UserRole role) ? role : UserRole.None;
    }

    private async Task<IdentityResult> ReplaceRolesAsync(ApplicationUser user, UserRole newRole)
    {
        IList<string> existingRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        if (existingRoles.Count > 0)
        {
            IdentityResult removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles).ConfigureAwait(false);
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }
        }

        return await _userManager.AddToRoleAsync(user, newRole.ToString()).ConfigureAwait(false);
    }

    private async Task<bool> EmailExistsAsync(
        string normalizedEmail,
        string? excludedUserId,
        CancellationToken cancellationToken)
    {
        IQueryable<ApplicationUser> users = _userManager.Users;

        // Some unit tests use an in-memory IQueryable without IAsyncQueryProvider.
        if (users.Provider is not Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider)
        {
            return users.Any(user =>
                user.NormalizedEmail == normalizedEmail
                && (excludedUserId == null || user.Id != excludedUserId));
        }

        return await users
            .AnyAsync(
                user => user.NormalizedEmail == normalizedEmail
                    && (excludedUserId == null || user.Id != excludedUserId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<string?> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        if (user.EmailConfirmed || string.IsNullOrWhiteSpace(user.Email))
        {
            return null;
        }

        return await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
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
            "RoleNotFound" => new ResultError("users.role_not_found", "The requested role does not exist."),
            "PasswordTooShort" => new ResultError("users.password_too_short", identityError.Description),
            _ => new ResultError("users.identity_error", identityError.Description),
        };
    }

    private sealed record UserRoleAssignment(string UserId, string? RoleName);
}
