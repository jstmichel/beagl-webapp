// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.EmailProviders.Dtos;
using Beagl.Application.EmailProviders.Services;
using Beagl.Domain.EmailProviders;
using Beagl.Domain.Results;
using FluentAssertions;
using Moq;

namespace Beagl.Application.Tests.EmailProviders.Services;

public class EmailProviderConfigServiceTests
{
    [Fact]
    public async Task GetActiveAsync_WhenNoConfigExists_ShouldReturnNull()
    {
        // Arrange
        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailProviderConfig?)null);

        EmailProviderConfigService service = new(repositoryMock.Object);

        // Act
        EmailProviderConfigDto? result = await service.GetActiveAsync(CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveAsync_WhenConfigExists_ShouldReturnDtoWithMaskedApiKey()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        EmailProviderConfig config = new(id, "xkeysib-abc01234", "noreply@example.com", "Beagl CRM");

        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        EmailProviderConfigService service = new(repositoryMock.Object);

        // Act
        EmailProviderConfigDto? result = await service.GetActiveAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.SenderEmail.Should().Be("noreply@example.com");
        result.SenderName.Should().Be("Beagl CRM");
        result.MaskedApiKey.Should().EndWith("1234");
        result.MaskedApiKey.Should().NotContain("xkeysib");
    }

    [Fact]
    public async Task GetActiveAsync_WhenApiKeyIsShort_ShouldMaskEntireKey()
    {
        // Arrange
        EmailProviderConfig config = new(Guid.NewGuid(), "abc", "noreply@example.com", "Test");

        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        EmailProviderConfigService service = new(repositoryMock.Object);

        // Act
        EmailProviderConfigDto? result = await service.GetActiveAsync(CancellationToken.None);

        // Assert
        result!.MaskedApiKey.Should().Be("***");
    }

    [Fact]
    public async Task SaveAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        EmailProviderConfigService service = new(repositoryMock.Object);

        // Act
        Func<Task> act = async () => await service.SaveAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", "sender@example.com", "Test Sender", "email_config.api_key_required")]
    [InlineData("  ", "sender@example.com", "Test Sender", "email_config.api_key_required")]
    [InlineData("xkeysib-abc", "", "Test Sender", "email_config.sender_email_required")]
    [InlineData("xkeysib-abc", "not-an-email", "Test Sender", "email_config.sender_email_invalid")]
    [InlineData("xkeysib-abc", "sender@example.com", "", "email_config.sender_name_required")]
    public async Task SaveAsync_WithInvalidInput_ShouldReturnValidationFailure(
        string apiKey,
        string senderEmail,
        string senderName,
        string expectedCode)
    {
        // Arrange
        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        EmailProviderConfigService service = new(repositoryMock.Object);
        SaveEmailProviderConfigRequest request = new(apiKey, senderEmail, senderName);

        // Act
        Result<EmailProviderConfigDto> result = await service.SaveAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(expectedCode);
        repositoryMock.Verify(r => r.SaveAsync(It.IsAny<EmailProviderConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_WhenNoExistingConfig_ShouldCreateNewRecord()
    {
        // Arrange
        EmailProviderConfig? capturedConfig = null;

        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailProviderConfig?)null);
        repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<EmailProviderConfig>(), It.IsAny<CancellationToken>()))
            .Callback<EmailProviderConfig, CancellationToken>((c, _) => capturedConfig = c)
            .ReturnsAsync((EmailProviderConfig c, CancellationToken _) => c);

        EmailProviderConfigService service = new(repositoryMock.Object);
        SaveEmailProviderConfigRequest request = new(" xkeysib-abc ", " sender@example.com ", " Beagl CRM ");

        // Act
        Result<EmailProviderConfigDto> result = await service.SaveAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedConfig.Should().NotBeNull();
        capturedConfig!.Id.Should().NotBe(Guid.Empty);
        capturedConfig.ApiKey.Should().Be("xkeysib-abc");
        capturedConfig.SenderEmail.Should().Be("sender@example.com");
        capturedConfig.SenderName.Should().Be("Beagl CRM");
    }

    [Fact]
    public async Task SaveAsync_WhenExistingConfigPresent_ShouldUpdateWithSameId()
    {
        // Arrange
        Guid existingId = Guid.NewGuid();
        EmailProviderConfig existing = new(existingId, "old-key", "old@example.com", "Old Name");
        EmailProviderConfig? capturedConfig = null;

        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<EmailProviderConfig>(), It.IsAny<CancellationToken>()))
            .Callback<EmailProviderConfig, CancellationToken>((c, _) => capturedConfig = c)
            .ReturnsAsync((EmailProviderConfig c, CancellationToken _) => c);

        EmailProviderConfigService service = new(repositoryMock.Object);
        SaveEmailProviderConfigRequest request = new("new-key", "new@example.com", "New Name");

        // Act
        Result<EmailProviderConfigDto> result = await service.SaveAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedConfig!.Id.Should().Be(existingId);
        capturedConfig.ApiKey.Should().Be("new-key");
        capturedConfig.SenderEmail.Should().Be("new@example.com");
        capturedConfig.SenderName.Should().Be("New Name");
    }

    [Fact]
    public async Task SaveAsync_WhenSuccessful_ShouldReturnDtoWithMaskedApiKey()
    {
        // Arrange
        Mock<IEmailProviderConfigRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailProviderConfig?)null);
        repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<EmailProviderConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailProviderConfig c, CancellationToken _) => c);

        EmailProviderConfigService service = new(repositoryMock.Object);
        SaveEmailProviderConfigRequest request = new("xkeysib-abc01234", "sender@example.com", "Beagl CRM");

        // Act
        Result<EmailProviderConfigDto> result = await service.SaveAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.MaskedApiKey.Should().EndWith("1234");
        result.Value.MaskedApiKey.Should().NotContain("xkeysib");
    }
}
