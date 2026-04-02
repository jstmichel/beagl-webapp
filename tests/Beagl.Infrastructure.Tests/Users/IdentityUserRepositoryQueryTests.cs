// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
using Beagl.Infrastructure.Users;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Beagl.Infrastructure.Tests.Users;

public class IdentityUserRepositoryQueryTests
{
    [Fact]
    public async Task HasAnyAdministratorAsync_WhenNoAdministratorRoleAssignmentExists_ShouldReturnFalse()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedRolesAsync(
            new ApplicationRole
            {
                Id = "role-employee",
                Name = UserRole.Employee.ToString(),
                NormalizedName = UserRole.Employee.ToString().ToUpperInvariant(),
            });
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: true, lockedOut: false));

        // Act
        bool hasAdministrator = await harness.Repository.HasAnyAdministratorAsync(CancellationToken.None);

        // Assert
        hasAdministrator.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyAdministratorAsync_WhenAdministratorRoleAssignmentExists_ShouldReturnTrue()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedRolesAsync(
            new ApplicationRole
            {
                Id = "role-admin",
                Name = UserRole.Administrator.ToString(),
                NormalizedName = UserRole.Administrator.ToString().ToUpperInvariant(),
            });
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: true, lockedOut: false));
        await harness.SeedUserRolesAsync(new IdentityUserRole<string> { UserId = "u1", RoleId = "role-admin" });

        // Act
        bool hasAdministrator = await harness.Repository.HasAnyAdministratorAsync(CancellationToken.None);

        // Assert
        hasAdministrator.Should().BeTrue();
    }

    [Fact]
    public async Task GetMetricsAsync_WithNoUsers_ShouldReturnZeroCounts()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();

        // Act
        UsersMetrics metrics = await harness.Repository.GetMetricsAsync(CancellationToken.None);

        // Assert
        metrics.TotalUsers.Should().Be(0);
        metrics.PendingConfirmationUsers.Should().Be(0);
        metrics.LockedOutUsers.Should().Be(0);
    }

    [Fact]
    public async Task GetMetricsAsync_WithMixedUsers_ShouldReturnCorrectCounts()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(
            MakeUser("u1", "alice", confirmed: true, lockedOut: false),
            MakeUser("u2", "bob", confirmed: false, lockedOut: false),
            MakeUser("u3", "charlie", confirmed: true, lockedOut: true));

        // Act
        UsersMetrics metrics = await harness.Repository.GetMetricsAsync(CancellationToken.None);

        // Assert
        metrics.TotalUsers.Should().Be(3);
        metrics.PendingConfirmationUsers.Should().Be(1);
        metrics.LockedOutUsers.Should().Be(1);
    }

    [Fact]
    public async Task GetPageAsync_WithMultipleUsers_ShouldReturnOrderedByUserName()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(
            MakeUser("u3", "charlie", confirmed: true, lockedOut: false),
            MakeUser("u1", "alice", confirmed: true, lockedOut: false),
            MakeUser("u2", "bob", confirmed: true, lockedOut: false));

        GetUsersPageQuery query = new(null, 1, 10);

        // Act
        UsersPage page = await harness.Repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        page.TotalCount.Should().Be(3);
        page.Users.Should().HaveCount(3);
        page.Users[0].UserName.Should().Be("alice");
        page.Users[1].UserName.Should().Be("bob");
        page.Users[2].UserName.Should().Be("charlie");
    }

    [Fact]
    public async Task GetPageAsync_WithAssignedRoles_ShouldMapRolesFromBatchQuery()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedRolesAsync(
            new ApplicationRole { Id = "role-admin", Name = UserRole.Administrator.ToString(), NormalizedName = UserRole.Administrator.ToString().ToUpperInvariant() },
            new ApplicationRole { Id = "role-employee", Name = UserRole.Employee.ToString(), NormalizedName = UserRole.Employee.ToString().ToUpperInvariant() });
        await harness.SeedUsersAsync(
            MakeUser("u1", "alice", confirmed: true, lockedOut: false),
            MakeUser("u2", "bob", confirmed: true, lockedOut: false),
            MakeUser("u3", "charlie", confirmed: true, lockedOut: false));
        await harness.SeedUserRolesAsync(
            new IdentityUserRole<string> { UserId = "u1", RoleId = "role-admin" },
            new IdentityUserRole<string> { UserId = "u2", RoleId = "role-employee" });

        GetUsersPageQuery query = new(null, 1, 10);

        // Act
        UsersPage page = await harness.Repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        page.Users.Should().HaveCount(3);
        page.Users[0].Role.Should().Be(UserRole.Administrator);
        page.Users[1].Role.Should().Be(UserRole.Employee);
        page.Users[2].Role.Should().Be(UserRole.None);
    }

    [Fact]
    public async Task GetPageAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(
            MakeUser("u1", "alice", confirmed: true, lockedOut: false),
            MakeUser("u2", "bob", confirmed: true, lockedOut: false),
            MakeUser("u3", "charlie", confirmed: true, lockedOut: false),
            MakeUser("u4", "diana", confirmed: true, lockedOut: false));

        GetUsersPageQuery query = new(null, 2, 2);

        // Act
        UsersPage page = await harness.Repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        page.TotalCount.Should().Be(4);
        page.PageNumber.Should().Be(2);
        page.PageSize.Should().Be(2);
        page.Users.Should().HaveCount(2);
        page.Users[0].UserName.Should().Be("charlie");
        page.Users[1].UserName.Should().Be("diana");
    }

    [Fact]
    public async Task GetPageAsync_WhenNoUsersExist_ShouldReturnEmptyPage()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        GetUsersPageQuery query = new(null, 1, 10);

        // Act
        UsersPage page = await harness.Repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        page.TotalCount.Should().Be(0);
        page.PageNumber.Should().Be(1);
        page.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPageAsync_WhenUserHasMultipleRoles_ShouldReturnFirstRoleOnly()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedRolesAsync(
            new ApplicationRole
            {
                Id = "role-admin",
                Name = UserRole.Administrator.ToString(),
                NormalizedName = UserRole.Administrator.ToString().ToUpperInvariant(),
            },
            new ApplicationRole
            {
                Id = "role-employee",
                Name = UserRole.Employee.ToString(),
                NormalizedName = UserRole.Employee.ToString().ToUpperInvariant(),
            });
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: true, lockedOut: false));
        await harness.SeedUserRolesAsync(
            new IdentityUserRole<string> { UserId = "u1", RoleId = "role-admin" },
            new IdentityUserRole<string> { UserId = "u1", RoleId = "role-employee" });

        GetUsersPageQuery query = new(null, 1, 10);

        // Act
        UsersPage page = await harness.Repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        page.Users.Should().ContainSingle();
        page.Users[0].Role.Should().Be(UserRole.Administrator);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnMappedUser()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: true, lockedOut: false));

        // Act
        UserAccount? user = await harness.Repository.GetByIdAsync("u1", CancellationToken.None);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be("u1");
        user.UserName.Should().Be("alice");
        user.EmailConfirmed.Should().BeTrue();
        user.IsLockedOut.Should().BeFalse();
        user.Role.Should().Be(UserRole.None);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();

        // Act
        UserAccount? user = await harness.Repository.GetByIdAsync("non-existent", CancellationToken.None);

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithNullFieldsAndActiveLockout_ShouldMapFallbackValues()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(
            new ApplicationUser
            {
                Id = "u-lock",
                UserName = null,
                NormalizedUserName = null,
                Email = null,
                NormalizedEmail = null,
                EmailConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.UtcNow.AddHours(1),
            });

        // Act
        UserAccount? user = await harness.Repository.GetByIdAsync("u-lock", CancellationToken.None);

        // Assert
        user.Should().NotBeNull();
        user!.UserName.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.IsLockedOut.Should().BeTrue();
        user.Role.Should().Be(UserRole.None);
    }

    [Fact]
    public async Task ConfirmAccountAsync_WhenUserIsUnconfirmed_ShouldPersistConfirmedState()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: false, lockedOut: false));

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await harness.Repository.ConfirmAccountAsync("u1", CancellationToken.None);
        UserAccount? reloadedUser = await harness.Repository.GetByIdAsync("u1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EmailConfirmed.Should().BeTrue();
        reloadedUser.Should().NotBeNull();
        reloadedUser!.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAccountAsync_WhenUserIsMissing_ShouldReturnFailureResult()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();

        // Act
        Beagl.Domain.Results.Result<UserAccount> result = await harness.Repository.ConfirmAccountAsync("missing", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_WhenUserIsUnconfirmedWithEmail_ShouldReturnToken()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: false, lockedOut: false));

        // Act
        Beagl.Domain.Results.Result<string> result = await harness.Repository.GenerateEmailConfirmationTokenAsync("u1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ConfirmAccountByTokenAsync_WhenTokenIsValid_ShouldConfirmUserAndReturnSuccess()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: false, lockedOut: false));

        Beagl.Domain.Results.Result<string> tokenResult = await harness.Repository.GenerateEmailConfirmationTokenAsync("u1", CancellationToken.None);
        tokenResult.IsSuccess.Should().BeTrue();

        // Act
        Beagl.Domain.Results.Result result = await harness.Repository.ConfirmAccountByTokenAsync("u1", tokenResult.Value!, CancellationToken.None);
        UserAccount? reloadedUser = await harness.Repository.GetByIdAsync("u1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reloadedUser.Should().NotBeNull();
        reloadedUser!.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordByRecoveryCodeAsync_WhenCodeDoesNotMatch_ShouldReturnFailure()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: true, lockedOut: false));

        // Act
        Beagl.Domain.Results.Result result = await harness.Repository.ResetPasswordByRecoveryCodeAsync("XYZABC", "NewP@ssw0rd", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_recovery_code");
    }

    [Fact]
    public async Task ResetPasswordByRecoveryCodeAsync_WhenCodeMatches_ShouldResetPasswordAndClearCode()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        ApplicationUser user = MakeUser("u1", "alice", confirmed: true, lockedOut: false);
        user.RecoveryCode = "ABCDEF";
        await harness.SeedUsersAsync(user);

        // Act
        Beagl.Domain.Results.Result result = await harness.Repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd!1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ApplicationUser? reloadedUser = await harness.FindUserByIdAsync("u1");
        reloadedUser.Should().NotBeNull();
        reloadedUser!.RecoveryCode.Should().BeNull();
    }

    [Fact]
    public async Task GenerateRecoveryCodeAsync_WhenUserExists_ShouldPersistCode()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        await harness.SeedUsersAsync(MakeUser("u1", "alice", confirmed: true, lockedOut: false));

        // Act
        Beagl.Domain.Results.Result result = await harness.Repository.GenerateRecoveryCodeAsync("u1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ApplicationUser? reloadedUser = await harness.FindUserByIdAsync("u1");
        reloadedUser.Should().NotBeNull();
        reloadedUser!.RecoveryCode.Should().NotBeNull();
        reloadedUser.RecoveryCode.Should().HaveLength(6);
        reloadedUser.RecoveryCode.Should().MatchRegex("^[A-Z]+$");
    }

    [Fact]
    public async Task ClearRecoveryCodeAsync_WhenUserHasCode_ShouldPersistNull()
    {
        // Arrange
        await using EfTestHarness harness = EfTestHarness.Create();
        ApplicationUser user = MakeUser("u1", "alice", confirmed: true, lockedOut: false);
        user.RecoveryCode = "ABCDEF";
        await harness.SeedUsersAsync(user);

        // Act
        await harness.Repository.ClearRecoveryCodeAsync("u1", CancellationToken.None);

        // Assert
        ApplicationUser? reloadedUser = await harness.FindUserByIdAsync("u1");
        reloadedUser.Should().NotBeNull();
        reloadedUser!.RecoveryCode.Should().BeNull();
    }

    private static ApplicationUser MakeUser(string id, string username, bool confirmed, bool lockedOut) =>
        new()
        {
            Id = id,
            UserName = username,
            NormalizedUserName = username.ToUpperInvariant(),
            Email = $"{username}@example.com",
            NormalizedEmail = $"{username}@example.com".ToUpperInvariant(),
            EmailConfirmed = confirmed,
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = lockedOut,
            LockoutEnd = lockedOut ? DateTimeOffset.UtcNow.AddHours(1) : null,
        };

    private sealed class EfTestHarness : IAsyncDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityUserRepository Repository { get; }

        private EfTestHarness(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            Repository = new IdentityUserRepository(userManager, context);
        }

        public static EfTestHarness Create()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            ApplicationDbContext context = new(options);

            UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext> store = new(context);
            UserManager<ApplicationUser> userManager = new(
                store,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object,
                new Logger<UserManager<ApplicationUser>>(new LoggerFactory()));

            userManager.RegisterTokenProvider(TokenOptions.DefaultProvider, new FakeEmailTokenProvider());

            return new EfTestHarness(context, userManager);
        }

        public async Task SeedUsersAsync(params ApplicationUser[] users)
        {
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        public async Task SeedRolesAsync(params ApplicationRole[] roles)
        {
            _context.Roles.AddRange(roles);
            await _context.SaveChangesAsync();
        }

        public async Task SeedUserRolesAsync(params IdentityUserRole<string>[] userRoles)
        {
            _context.Set<IdentityUserRole<string>>().AddRange(userRoles);
            await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async ValueTask DisposeAsync()
        {
            _userManager.Dispose();
            await _context.DisposeAsync();
        }

        private sealed class FakeEmailTokenProvider : IUserTwoFactorTokenProvider<ApplicationUser>
        {
            public Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
                => Task.FromResult($"{user.Id}:{purpose}");

            public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
                => Task.FromResult(token == $"{user.Id}:{purpose}");

            public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
                => Task.FromResult(false);
        }
    }
}
