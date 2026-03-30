// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Setup.Dtos;
using Beagl.Application.Setup.Services;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using FluentAssertions;
using Moq;

namespace Beagl.Application.Tests.Setup.Services;

public class InitialSetupServiceTests
{
    [Fact]
    public async Task IsSetupRequiredAsync_WhenNoAdministratorExists_ShouldReturnTrue()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.HasAnyAdministratorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        SetupStatusCache cache = new();
        InitialSetupService service = new(userRepositoryMock.Object, cache);

        // Act
        bool result = await service.IsSetupRequiredAsync(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSetupRequiredAsync_WhenAdministratorExists_ShouldCacheResult()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.HasAnyAdministratorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        SetupStatusCache cache = new();
        InitialSetupService service = new(userRepositoryMock.Object, cache);

        // Act
        bool firstCall = await service.IsSetupRequiredAsync(CancellationToken.None);
        bool secondCall = await service.IsSetupRequiredAsync(CancellationToken.None);

        // Assert
        firstCall.Should().BeFalse();
        secondCall.Should().BeFalse();
        userRepositoryMock.Verify(
            repository => repository.HasAnyAdministratorAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsSetupRequiredAsync_WhenCacheAlreadyComplete_ShouldSkipRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        SetupStatusCache cache = new();
        cache.MarkSetupComplete();
        InitialSetupService service = new(userRepositoryMock.Object, cache);

        // Act
        bool result = await service.IsSetupRequiredAsync(CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        userRepositoryMock.Verify(
            repository => repository.HasAnyAdministratorAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CompleteInitialSetupAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        InitialSetupService service = new(userRepositoryMock.Object, new SetupStatusCache());

        // Act
        Func<Task> act = async () => await service.CompleteInitialSetupAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CompleteInitialSetupAsync_WhenSetupAlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.HasAnyAdministratorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InitialSetupService service = new(userRepositoryMock.Object, new SetupStatusCache());
        CompleteInitialSetupRequest request = new("admin", "admin@example.com", "Password123!");

        // Act
        Result result = await service.CompleteInitialSetupAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("setup.already_completed");
        userRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CompleteInitialSetupAsync_WhenDataIsValid_ShouldCreateVerifiedAdministrator()
    {
        // Arrange
        CreateUserAccount? capturedCreateUser = null;

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.HasAnyAdministratorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<CreateUserAccount, CancellationToken>((createUser, _) => capturedCreateUser = createUser)
            .ReturnsAsync(Result.Success(new UserAccount("user-1", "admin", "admin@example.com", null, true, false, UserRole.Administrator)));

        InitialSetupService service = new(userRepositoryMock.Object, new SetupStatusCache());
        CompleteInitialSetupRequest request = new(" admin ", " admin@example.com ", "Password123!");

        // Act
        Result result = await service.CompleteInitialSetupAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedCreateUser.Should().NotBeNull();
        capturedCreateUser!.UserName.Should().Be("admin");
        capturedCreateUser.Email.Should().Be("admin@example.com");
        capturedCreateUser.PhoneNumber.Should().BeNull();
        capturedCreateUser.Role.Should().Be(UserRole.Administrator);
        capturedCreateUser.EmailConfirmed.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "admin@example.com", "Password123!", "setup.user_name_required")]
    [InlineData("admin", "", "Password123!", "setup.email_required")]
    [InlineData("admin", "bad-email", "Password123!", "setup.invalid_email")]
    [InlineData("admin", "admin@example.com", "", "setup.password_required")]
    [InlineData("admin", "admin@example.com", "short", "setup.password_too_short")]
    public async Task CompleteInitialSetupAsync_WithInvalidInput_ShouldReturnValidationFailure(
        string userName,
        string email,
        string password,
        string expectedCode)
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        InitialSetupService service = new(userRepositoryMock.Object, new SetupStatusCache());

        CompleteInitialSetupRequest request = new(userName, email, password);

        // Act
        Result result = await service.CompleteInitialSetupAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(expectedCode);
        userRepositoryMock.Verify(
            repository => repository.HasAnyAdministratorAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
