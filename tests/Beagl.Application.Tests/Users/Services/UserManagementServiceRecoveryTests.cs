// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.EmailProviders.Services;
using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beagl.Application.Tests.Users.Services;

public class UserManagementServiceRecoveryTests
{
    [Fact]
    public async Task RequestRecoveryCodeAsync_WithEmptyIdentifier_ShouldReturnSuccess()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            repository => repository.FindByIdentifierAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_WithWhitespaceIdentifier_ShouldReturnSuccess()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("   ", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            repository => repository.FindByIdentifierAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_WhenUserNotFound_ShouldReturnSuccess()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.FindByIdentifierAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount?)null);

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("unknown", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            repository => repository.GenerateRecoveryCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_WhenUserFound_ShouldGenerateCode()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.FindByIdentifierAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee));

        userRepositoryMock
            .Setup(repository => repository.GenerateRecoveryCodeAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("alex", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            repository => repository.GenerateRecoveryCodeAsync("user-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_ShouldTrimIdentifier()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.FindByIdentifierAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee));

        userRepositoryMock
            .Setup(repository => repository.GenerateRecoveryCodeAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("  alex  ", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            repository => repository.FindByIdentifierAsync("alex", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecoverAccountAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Func<Task> act = async () => await service.RecoverAccountAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecoverAccountAsync_WithEmptyCode_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("", "NewP@ssw0rd");

        // Act
        Result result = await service.RecoverAccountAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.recovery_code_required");
    }

    [Fact]
    public async Task RecoverAccountAsync_WithInvalidCodeLength_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("ABC", "NewP@ssw0rd");

        // Act
        Result result = await service.RecoverAccountAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_recovery_code");
    }

    [Fact]
    public async Task RecoverAccountAsync_WithEmptyPassword_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("ABCDEF", "");

        // Act
        Result result = await service.RecoverAccountAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.password_required");
    }

    [Fact]
    public async Task RecoverAccountAsync_WithShortPassword_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("ABCDEF", "short");

        // Act
        Result result = await service.RecoverAccountAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.password_too_short");
    }

    [Fact]
    public async Task RecoverAccountAsync_WithValidInput_ShouldResetPassword()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("ABCDEF", "NewP@ssw0rd");

        // Act
        Result result = await service.RecoverAccountAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            repository => repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecoverAccountAsync_ShouldUppercaseCode()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("abcdef", "NewP@ssw0rd");

        // Act
        Result result = await service.RecoverAccountAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            repository => repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecoverAccountAsync_WhenCodeInvalid_ShouldReturnFailure()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.ResetPasswordByRecoveryCodeAsync("XYZABC", "NewP@ssw0rd", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("users.invalid_recovery_code", "The recovery code is invalid.")));

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("XYZABC", "NewP@ssw0rd");

        // Act
        Result result = await service.RecoverAccountAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_recovery_code");
    }

    [Fact]
    public async Task RecoverAccountAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd", cancellationToken))
            .ReturnsAsync(Result.Success());

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        RecoverAccountRequest request = new("ABCDEF", "NewP@ssw0rd");

        // Act
        _ = await service.RecoverAccountAsync(request, cancellationToken);

        // Assert
        userRepositoryMock.Verify(
            repository => repository.ResetPasswordByRecoveryCodeAsync("ABCDEF", "NewP@ssw0rd", cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUsersPageAsync_ShouldMapRecoveryCodeIndicator()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetPageAsync(It.IsAny<GetUsersPageQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsersPage(
                [
                    new UserAccount("user-1", "alex", "alex@example.com", "555-0100", true, false, UserRole.Employee, RecoveryCode: "ABCDEF"),
                    new UserAccount("user-2", "bob", "bob@example.com", "555-0200", true, false, UserRole.Employee),
                ],
                2, 1, 10));

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        GetUsersPageRequest request = new(null, 1, 10);

        // Act
        UsersPageDto result = await service.GetUsersPageAsync(request, CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(2);
        result.Users[0].HasRecoveryCode.Should().BeTrue();
        result.Users[1].HasRecoveryCode.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldMapRecoveryCode()
    {
        // Arrange
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee, RecoveryCode: "XYZABC"));

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result<UserDetailsDto> result = await service.GetUserByIdAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RecoveryCode.Should().Be("XYZABC");
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_WhenUserHasEmailAndCode_ShouldSendRecoveryCodeEmail()
    {
        // Arrange
        UserAccount user = new("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee);
        UserAccount updatedUser = new("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee, RecoveryCode: "ABC123");

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.FindByIdentifierAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(repository => repository.GenerateRecoveryCodeAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        citizenProfileRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CitizenProfile(Guid.NewGuid(), "user-1", "Alex", "Doe", null, null, CommunicationPreference.Email, LanguagePreference.French));

        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        emailTemplateServiceMock
            .Setup(t => t.RenderRecoveryCode(LanguagePreference.French, It.IsAny<RecoveryCodeTokens>()))
            .Returns(new EmailTemplateResult("Subject FR", "<p>Body FR</p>"));

        Mock<IEmailSender> emailSenderMock = new();
        emailSenderMock
            .Setup(s => s.SendAsync("alex@example.com", "alex", "Subject FR", "<p>Body FR</p>", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("alex", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        emailSenderMock.Verify(
            s => s.SendAsync("alex@example.com", "alex", "Subject FR", "<p>Body FR</p>", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_WhenUserHasNoEmail_ShouldNotSendEmail()
    {
        // Arrange
        UserAccount user = new("user-1", "alex", "", "555-0100", true, false, UserRole.Citizen);
        UserAccount updatedUser = new("user-1", "alex", "", "555-0100", true, false, UserRole.Citizen, RecoveryCode: "ABC123");

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.FindByIdentifierAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(repository => repository.GenerateRecoveryCodeAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("alex", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        emailSenderMock.Verify(
            s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_WhenNoProfile_ShouldFallBackToEnglish()
    {
        // Arrange
        UserAccount user = new("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee);
        UserAccount updatedUser = new("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee, RecoveryCode: "ABC123");

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.FindByIdentifierAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(repository => repository.GenerateRecoveryCodeAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();
        citizenProfileRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CitizenProfile?)null);

        RecoveryCodeTokens? capturedTokens = null;
        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        emailTemplateServiceMock
            .Setup(t => t.RenderRecoveryCode(LanguagePreference.None, It.IsAny<RecoveryCodeTokens>()))
            .Callback<LanguagePreference, RecoveryCodeTokens>((_, t) => capturedTokens = t)
            .Returns(new EmailTemplateResult("Subject EN", "<p>Body EN</p>"));

        Mock<IEmailSender> emailSenderMock = new();
        emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("alex", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedTokens.Should().NotBeNull();
        capturedTokens!.UserName.Should().Be("alex");
        capturedTokens.RecoveryCode.Should().Be("ABC123");
        emailTemplateServiceMock.Verify(
            t => t.RenderRecoveryCode(LanguagePreference.None, It.IsAny<RecoveryCodeTokens>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestRecoveryCodeAsync_WhenEmailSendFails_ShouldStillReturnSuccess()
    {
        // Arrange
        UserAccount user = new("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee);
        UserAccount updatedUser = new("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee, RecoveryCode: "ABC123");

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(repository => repository.FindByIdentifierAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(repository => repository.GenerateRecoveryCodeAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        Mock<ICitizenProfileRepository> citizenProfileRepositoryMock = new();

        Mock<IEmailTemplateService> emailTemplateServiceMock = new();
        emailTemplateServiceMock
            .Setup(t => t.RenderRecoveryCode(It.IsAny<LanguagePreference>(), It.IsAny<RecoveryCodeTokens>()))
            .Returns(new EmailTemplateResult("Subject", "<p>Body</p>"));

        Mock<IEmailSender> emailSenderMock = new();
        emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new ResultError("email.send_failed", "Failed.")));

        UserManagementService service = new(userRepositoryMock.Object, citizenProfileRepositoryMock.Object, emailSenderMock.Object, emailTemplateServiceMock.Object, NullLogger<UserManagementService>.Instance);

        // Act
        Result result = await service.RequestRecoveryCodeAsync("alex", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
