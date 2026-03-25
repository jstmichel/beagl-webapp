// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Tests.Utilities;
using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beagl.Application.Tests.Users.Services;

public class UserManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Func<Task> act = async () => await service.CreateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Func<Task> act = async () => await service.UpdateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetUsersPageAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Func<Task> act = async () => await service.GetUsersPageAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetUsersMetricsAsync_WhenRepositoryReturnsMetrics_ShouldMapResponse()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetMetricsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsersMetrics(27, 4, 2));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        UsersMetricsDto result = await service.GetUsersMetricsAsync(CancellationToken.None);

        // Assert
        result.TotalUsers.Should().Be(27);
        result.PendingConfirmationUsers.Should().Be(4);
        result.LockedOutUsers.Should().Be(2);
    }

    [Fact]
    public async Task GetUsersMetricsAsync_ShouldForwardCancellationTokenToRepository()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetMetricsAsync(cancellationToken))
            .ReturnsAsync(new UsersMetrics(1, 0, 0));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        _ = await service.GetUsersMetricsAsync(cancellationToken);

        // Assert
        userRepositoryMock.Verify(repository => repository.GetMetricsAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetUsersPageAsync_WithInvalidPagingAndWhitespaceSearch_ShouldNormalizeQuery()
    {
        // Arrange
        GetUsersPageQuery? capturedQuery = null;

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetPageAsync(It.IsAny<GetUsersPageQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetUsersPageQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(new UsersPage(
                [new UserAccount("user-1", "alex", "alex@example.com", "555-0100", true, false, UserRole.Employee)],
                1,
                1,
                10));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        GetUsersPageRequest request = new("  alex  ", 0, 0);

        // Act
        UsersPageDto result = await service.GetUsersPageAsync(request, CancellationToken.None);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.SearchTerm.Should().Be("alex");
        capturedQuery.PageNumber.Should().Be(1);
        capturedQuery.PageSize.Should().Be(10);

        result.TotalCount.Should().Be(1);
        result.Users.Should().HaveCount(1);
        result.Users[0].Email.Should().Be("alex@example.com");
    }

    [Fact]
    public async Task GetUsersPageAsync_WithWhitespaceOnlySearch_ShouldSetQuerySearchToNull()
    {
        // Arrange
        GetUsersPageQuery? capturedQuery = null;

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetPageAsync(It.IsAny<GetUsersPageQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetUsersPageQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(new UsersPage([], 0, 1, 10));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        GetUsersPageRequest request = new("   ", 1, 10);

        // Act
        _ = await service.GetUsersPageAsync(request, CancellationToken.None);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.SearchTerm.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersPageAsync_WithPageSizeOverMaximum_ShouldClampToOneHundred()
    {
        // Arrange
        GetUsersPageQuery? capturedQuery = null;

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetPageAsync(It.IsAny<GetUsersPageQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetUsersPageQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(new UsersPage([], 0, 1, 100));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        GetUsersPageRequest request = new(null, 1, 101);

        // Act
        _ = await service.GetUsersPageAsync(request, CancellationToken.None);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithMissingIdentifier_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.GetUserByIdAsync(" ", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_id");
        userRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNullIdentifier_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.GetUserByIdAsync(null!, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_id");
        userRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenRepositoryReturnsNull_ShouldReturnNotFound()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount?)null);

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.GetUserByIdAsync("missing", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithWhitespaceAroundIdentifier_ShouldTrimBeforeRepositoryCall()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.GetUserByIdAsync("  user-1  ", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserName.Should().Be("alex");

        userRepositoryMock.Verify(repository => repository.GetByIdAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidEmail_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "invalid-email", null, "Password123!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_email");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRole_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "alex@example.com", "555-0100", "Password123!", UserRole.None);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_role");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithMissingUserName_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new(" ", "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.user_name_required");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithUserNameTooLong_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new(new string('a', 257), "alex@example.com", "555-0100", "Password123!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.user_name_too_long");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ForAdministratorWithoutEmail_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("admin", null, "555-0100", "Password123!", UserRole.Administrator);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.email_required");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ForEmployeeWithoutEmail_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("employee", null, "555-0100", "Password123!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.email_required");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithMissingPassword_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "alex@example.com", "555-0100", " ", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.password_required");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithShortPassword_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "alex@example.com", "555-0100", "Short1!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.password_too_short");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRepositorySucceeds_ShouldReturnCreatedUser()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-1", "alex", "alex@example.com", "+1234567890", false, false, UserRole.Employee, "confirmation-token")));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "alex@example.com", "+1234567890", "Password123!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-1");
        result.Value.UserName.Should().Be("alex");
        result.Value.Email.Should().Be("alex@example.com");
        result.Value.EmailConfirmationToken.Should().Be("confirmation-token");
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryFails_ShouldReturnSameError()
    {
        // Arrange
        ResultError expectedError = new("users.duplicate_email", "The email address is already in use.");

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserAccount>(expectedError));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "alex@example.com", "+1234567890", "Password123!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task CreateAsync_WithWhitespaceInput_ShouldTrimAndNormalizeBeforeRepositoryCall()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-1", "alex", "alex@example.com", null, false, false, UserRole.Employee)));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("  alex  ", "  alex@example.com  ", "  555-0100  ", "Password123!", UserRole.Employee);

        // Act
        _ = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        userRepositoryMock.Verify(
            repository => repository.CreateAsync(
                It.Is<CreateUserAccount>(createUser =>
                    createUser.UserName == "alex"
                    && createUser.Email == "alex@example.com"
                    && createUser.PhoneNumber == "555-0100"
                    && createUser.Role == UserRole.Employee),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ForCitizenWithoutPhone_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("citizen-1", null, null, "Password123!", UserRole.Citizen);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.phone_required");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ForEmployeeWithoutPhone_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("emp-1", "emp@example.com", null, "Password123!", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.phone_required");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenRepositoryReturnsNotFound_ShouldPropagateFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserAccount>(new ResultError("users.not_found", "The requested user could not be found.")));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("missing", "alex", "alex@example.com", "+1234567890", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidRole_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.None);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_role");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithMissingUserName_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", " ", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.user_name_required");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithUserNameTooLong_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", new string('a', 257), "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.user_name_too_long");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidEmail_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", "alex", "bad-email", "555-0100", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_email");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithMissingPhone_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", "alex", "alex@example.com", " ", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.phone_required");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ForAdministratorWithoutEmail_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", "admin", null, "555-0100", UserRole.Administrator);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.email_required");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ForEmployeeWithoutEmail_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", "employee", null, "555-0100", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.email_required");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenRepositorySucceeds_ShouldReturnMappedUser()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-1", "alex", "alex@example.com", "555-0100", true, false, UserRole.Employee)));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("user-1", "alex", "alex@example.com", "555-0100", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-1");
        result.Value.UserName.Should().Be("alex");
        result.Value.Email.Should().Be("alex@example.com");
    }

    [Fact]
    public async Task UpdateAsync_WithWhitespaceIdentifier_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("   ", "alex", "alex@example.com", "+1234567890", UserRole.Employee);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_id");
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithWhitespaceInput_ShouldTrimAndNormalizeBeforeRepositoryCall()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee)));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        UpdateUserRequest request = new("  user-1  ", "  alex  ", "  alex@example.com  ", "  555-0100  ", UserRole.Employee);

        // Act
        _ = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        userRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.Is<UpdateUserAccount>(updateUser =>
                    updateUser.Id == "user-1"
                    && updateUser.UserName == "alex"
                    && updateUser.Email == "alex@example.com"
                    && updateUser.PhoneNumber == "555-0100"
                    && updateUser.Role == UserRole.Employee),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithMissingIdentifier_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.DeleteAsync(string.Empty, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_id");
        userRepositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithWhitespaceIdentifier_ShouldTrimBeforeRepositoryCall()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.DeleteAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.DeleteAsync("  user-1  ", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(repository => repository.DeleteAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRepositoryReturnsFailure_ShouldPropagateFailure()
    {
        // Arrange
        ResultError expectedError = new("users.not_found", "The requested user could not be found.");

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.DeleteAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(expectedError));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.DeleteAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task DeleteAsync_ShouldForwardCancellationTokenToRepository()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.DeleteAsync("user-1", cancellationToken))
            .ReturnsAsync(Result.Success());

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        _ = await service.DeleteAsync("user-1", cancellationToken);

        // Assert
        userRepositoryMock.Verify(repository => repository.DeleteAsync("user-1", cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ConfirmAccountAsync_WithMissingIdentifier_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.ConfirmAccountAsync(string.Empty, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_id");
        userRepositoryMock.Verify(repository => repository.ConfirmAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAccountAsync_WithWhitespaceIdentifier_ShouldTrimBeforeRepositoryCall()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.ConfirmAccountAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-1", "alex", "alex@example.com", "555-0100", true, false, UserRole.Employee)));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.ConfirmAccountAsync("  user-1  ", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EmailConfirmed.Should().BeTrue();
        userRepositoryMock.Verify(repository => repository.ConfirmAccountAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmAccountAsync_WhenRepositoryReturnsFailure_ShouldPropagateFailure()
    {
        // Arrange
        ResultError expectedError = new("users.not_found", "The requested user could not be found.");

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.ConfirmAccountAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserAccount>(expectedError));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.ConfirmAccountAsync("user-1", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task ConfirmAccountAsync_WhenSucceeds_ShouldLogAccountConfirmationWithEventId1004()
    {
        // Arrange
        FakeLogger<UserManagementService> fakeLogger = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.ConfirmAccountAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-123", "alex", "alex@example.com", "+1234567890", true, false, UserRole.Employee)));

        UserManagementService service = new(userRepositoryMock.Object, fakeLogger);

        // Act
        _ = await service.ConfirmAccountAsync("user-123", CancellationToken.None);

        // Assert
        fakeLogger.Logs.Should().NotBeEmpty();
        fakeLogger.Logs.Should().ContainSingle(log =>
            log.Level == LogLevel.Information
            && log.EventId.Id == 1004
            && log.Message.Contains("user-123"));
    }

    [Fact]
    public async Task CreateAsync_WhenSucceeds_ShouldLogUserCreatedWithEventId1001()
    {
        // Arrange
        FakeLogger<UserManagementService> fakeLogger = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-123", "alex", "alex@example.com", "+1234567890", false, false, UserRole.Employee)));

        UserManagementService service = new(userRepositoryMock.Object, fakeLogger);
        CreateUserRequest request = new("alex", "alex@example.com", "+1234567890", "Password123!", UserRole.Employee);

        // Act
        _ = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        fakeLogger.Logs.Should().NotBeEmpty();
        fakeLogger.Logs.Should().ContainSingle(log =>
            log.Level == LogLevel.Information
            && log.EventId.Id == 1001
            && log.Message.Contains("user-123"));
    }

    [Fact]
    public async Task UpdateAsync_WhenSucceeds_ShouldLogUserUpdatedWithEventId1002()
    {
        // Arrange
        FakeLogger<UserManagementService> fakeLogger = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<UpdateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-456", "bob", "bob@example.com", "555-0100", true, false, UserRole.Employee)));

        UserManagementService service = new(userRepositoryMock.Object, fakeLogger);
        UpdateUserRequest request = new("user-456", "bob", "bob@example.com", "555-0100", UserRole.Employee);

        // Act
        _ = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        fakeLogger.Logs.Should().NotBeEmpty();
        fakeLogger.Logs.Should().ContainSingle(log =>
            log.Level == LogLevel.Information
            && log.EventId.Id == 1002
            && log.Message.Contains("user-456"));
    }

    [Fact]
    public async Task DeleteAsync_WhenSucceeds_ShouldLogUserDeletedWithEventId1003()
    {
        // Arrange
        FakeLogger<UserManagementService> fakeLogger = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.DeleteAsync("user-789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        UserManagementService service = new(userRepositoryMock.Object, fakeLogger);

        // Act
        _ = await service.DeleteAsync("user-789", CancellationToken.None);

        // Assert
        fakeLogger.Logs.Should().NotBeEmpty();
        fakeLogger.Logs.Should().ContainSingle(log =>
            log.Level == LogLevel.Information
            && log.EventId.Id == 1003
            && log.Message.Contains("user-789"));
    }
}
