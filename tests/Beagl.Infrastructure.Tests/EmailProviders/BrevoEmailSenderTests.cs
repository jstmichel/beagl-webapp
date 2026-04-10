// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Net;
using Beagl.Domain.EmailProviders;
using Beagl.Domain.Results;
using Beagl.Infrastructure.EmailProviders;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace Beagl.Infrastructure.Tests.EmailProviders;

public class BrevoEmailSenderTests
{
    private static EmailProviderConfig CreateConfig()
    {
        return new EmailProviderConfig(
            Guid.NewGuid(),
            "test-api-key",
            "noreply@example.com",
            "Test Sender");
    }

    private static (Mock<IHttpClientFactory>, Mock<HttpMessageHandler>) CreateHttpMocks(
        HttpStatusCode statusCode,
        string responseBody = "")
    {
        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody),
            });

        HttpClient httpClient = new(handlerMock.Object);
        Mock<IHttpClientFactory> factoryMock = new();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        return (factoryMock, handlerMock);
    }

    [Fact]
    public async Task SendAsync_WhenNoProviderConfigured_ShouldReturnFailure()
    {
        // Arrange
        Mock<IEmailProviderConfigRepository> configRepositoryMock = new();
        configRepositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailProviderConfig?)null);

        Mock<IHttpClientFactory> factoryMock = new();
        BrevoEmailSender sender = new(
            factoryMock.Object,
            configRepositoryMock.Object,
            NullLogger<BrevoEmailSender>.Instance);

        // Act
        Result result = await sender.SendAsync(
            "user@example.com", "User", "Subject", "<p>Body</p>", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("email.no_provider_configured");
    }

    [Fact]
    public async Task SendAsync_WhenApiReturnsSuccess_ShouldReturnSuccess()
    {
        // Arrange
        EmailProviderConfig config = CreateConfig();
        Mock<IEmailProviderConfigRepository> configRepositoryMock = new();
        configRepositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        (Mock<IHttpClientFactory> factoryMock, _) = CreateHttpMocks(HttpStatusCode.Created);

        BrevoEmailSender sender = new(
            factoryMock.Object,
            configRepositoryMock.Object,
            NullLogger<BrevoEmailSender>.Instance);

        // Act
        Result result = await sender.SendAsync(
            "user@example.com", "User", "Subject", "<p>Body</p>", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WhenApiReturnsError_ShouldReturnFailure()
    {
        // Arrange
        EmailProviderConfig config = CreateConfig();
        Mock<IEmailProviderConfigRepository> configRepositoryMock = new();
        configRepositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        (Mock<IHttpClientFactory> factoryMock, _) = CreateHttpMocks(
            HttpStatusCode.BadRequest, """{"message":"Invalid email"}""");

        BrevoEmailSender sender = new(
            factoryMock.Object,
            configRepositoryMock.Object,
            NullLogger<BrevoEmailSender>.Instance);

        // Act
        Result result = await sender.SendAsync(
            "user@example.com", "User", "Subject", "<p>Body</p>", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("email.send_failed");
    }

    [Fact]
    public async Task SendAsync_WhenHttpRequestExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        EmailProviderConfig config = CreateConfig();
        Mock<IEmailProviderConfigRepository> configRepositoryMock = new();
        configRepositoryMock
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        HttpClient httpClient = new(handlerMock.Object);
        Mock<IHttpClientFactory> factoryMock = new();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        BrevoEmailSender sender = new(
            factoryMock.Object,
            configRepositoryMock.Object,
            NullLogger<BrevoEmailSender>.Instance);

        // Act
        Result result = await sender.SendAsync(
            "user@example.com", "User", "Subject", "<p>Body</p>", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("email.send_failed");
    }

    [Fact]
    public async Task SendAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        EmailProviderConfig config = CreateConfig();
        Mock<IEmailProviderConfigRepository> configRepositoryMock = new();
        configRepositoryMock
            .Setup(r => r.GetActiveAsync(token))
            .ReturnsAsync(config);

        (Mock<IHttpClientFactory> factoryMock, _) = CreateHttpMocks(HttpStatusCode.Created);

        BrevoEmailSender sender = new(
            factoryMock.Object,
            configRepositoryMock.Object,
            NullLogger<BrevoEmailSender>.Instance);

        // Act
        _ = await sender.SendAsync("user@example.com", "User", "Subject", "<p>Body</p>", token);

        // Assert
        configRepositoryMock.Verify(r => r.GetActiveAsync(token), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullHttpClientFactory_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new BrevoEmailSender(
            null!,
            new Mock<IEmailProviderConfigRepository>().Object,
            NullLogger<BrevoEmailSender>.Instance);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullEmailProviderConfigRepository_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new BrevoEmailSender(
            new Mock<IHttpClientFactory>().Object,
            null!,
            NullLogger<BrevoEmailSender>.Instance);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new BrevoEmailSender(
            new Mock<IHttpClientFactory>().Object,
            new Mock<IEmailProviderConfigRepository>().Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
