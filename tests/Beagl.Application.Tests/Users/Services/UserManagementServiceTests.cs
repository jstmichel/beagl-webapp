// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beagl.Application.Tests.Users.Services;

public class UserManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_WithInvalidEmail_ShouldReturnFailureWithoutCallingRepository()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "invalid-email", null, "Password123!");

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.invalid_email");
        userRepositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRepositorySucceeds_ShouldReturnCreatedUser()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<CreateUserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserAccount("user-1", "alex", "alex@example.com", null, false, false)));

        UserManagementService service = new(userRepositoryMock.Object, NullLogger<UserManagementService>.Instance);
        CreateUserRequest request = new("alex", "alex@example.com", null, "Password123!");

        // Act
        Result<UserDetailsDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-1");
        result.Value.UserName.Should().Be("alex");
        result.Value.Email.Should().Be("alex@example.com");
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
        UpdateUserRequest request = new("missing", "alex", "alex@example.com", null);

        // Act
        Result<UserDetailsDto> result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("users.not_found");
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
}