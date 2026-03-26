// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.EmailProviders;
using Beagl.Infrastructure.EmailProviders;
using Beagl.Infrastructure.EmailProviders.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Tests.EmailProviders;

public class EmailProviderConfigEntityTests
{
    [Fact]
    public void DefaultValues_ShouldBeEmptyStrings()
    {
        // Act
        EmailProviderConfigEntity entity = new();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
        entity.ApiKey.Should().Be(string.Empty);
        entity.SenderEmail.Should().Be(string.Empty);
        entity.SenderName.Should().Be(string.Empty);
    }

    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        EmailProviderConfigEntity entity = new()
        {
            Id = id,
            ApiKey = "xkeysib-abc",
            SenderEmail = "noreply@example.com",
            SenderName = "Beagl CRM",
        };

        // Assert
        entity.Id.Should().Be(id);
        entity.ApiKey.Should().Be("xkeysib-abc");
        entity.SenderEmail.Should().Be("noreply@example.com");
        entity.SenderName.Should().Be("Beagl CRM");
    }
}

public class EmailProviderConfigRepositoryTests
{
    [Fact]
    public void Constructor_WithNullDbContext_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new EmailProviderConfigRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetActiveAsync_WhenNoRecordExists_ShouldReturnNull()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        EmailProviderConfigRepository repository = new(dbContext);

        // Act
        EmailProviderConfig? result = await repository.GetActiveAsync(CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveAsync_WhenRecordExists_ShouldReturnMappedDomainObject()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.EmailProviderConfigurations.Add(new EmailProviderConfigEntity
        {
            Id = id,
            ApiKey = "xkeysib-abc",
            SenderEmail = "noreply@example.com",
            SenderName = "Beagl CRM",
        });
        await dbContext.SaveChangesAsync();

        EmailProviderConfigRepository repository = new(dbContext);

        // Act
        EmailProviderConfig? result = await repository.GetActiveAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.ApiKey.Should().Be("xkeysib-abc");
        result.SenderEmail.Should().Be("noreply@example.com");
        result.SenderName.Should().Be("Beagl CRM");
    }

    [Fact]
    public async Task SaveAsync_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        EmailProviderConfigRepository repository = new(dbContext);

        // Act
        Func<Task> act = async () => await repository.SaveAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_WhenNoExistingRecord_ShouldInsertNewRow()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        EmailProviderConfigRepository repository = new(dbContext);

        Guid id = Guid.NewGuid();
        EmailProviderConfig config = new(id, "xkeysib-newkey", "sender@example.com", "Test");

        // Act
        EmailProviderConfig saved = await repository.SaveAsync(config, CancellationToken.None);

        // Assert
        saved.Should().Be(config);
        int rowCount = await dbContext.EmailProviderConfigurations.CountAsync();
        rowCount.Should().Be(1);

        EmailProviderConfigEntity? persisted = await dbContext.EmailProviderConfigurations.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(id);
        persisted.ApiKey.Should().Be("xkeysib-newkey");
        persisted.SenderEmail.Should().Be("sender@example.com");
        persisted.SenderName.Should().Be("Test");
    }

    [Fact]
    public async Task SaveAsync_WhenRecordAlreadyExists_ShouldUpdateExistingRow()
    {
        // Arrange
        Guid existingId = Guid.NewGuid();
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.EmailProviderConfigurations.Add(new EmailProviderConfigEntity
        {
            Id = existingId,
            ApiKey = "old-key",
            SenderEmail = "old@example.com",
            SenderName = "Old Name",
        });
        await dbContext.SaveChangesAsync();

        EmailProviderConfigRepository repository = new(dbContext);
        EmailProviderConfig updated = new(existingId, "new-key", "new@example.com", "New Name");

        // Act
        EmailProviderConfig saved = await repository.SaveAsync(updated, CancellationToken.None);

        // Assert
        saved.Should().Be(updated);
        int rowCount = await dbContext.EmailProviderConfigurations.CountAsync();
        rowCount.Should().Be(1, "updating must not create a second row");

        EmailProviderConfigEntity? persisted = await dbContext.EmailProviderConfigurations.FirstOrDefaultAsync();
        persisted!.ApiKey.Should().Be("new-key");
        persisted.SenderEmail.Should().Be("new@example.com");
        persisted.SenderName.Should().Be("New Name");
    }

    [Fact]
    public async Task SaveAsync_WhenRecordAlreadyExists_ShouldReturnPassedConfig()
    {
        // Arrange
        Guid existingId = Guid.NewGuid();
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.EmailProviderConfigurations.Add(new EmailProviderConfigEntity
        {
            Id = existingId,
            ApiKey = "old-key",
            SenderEmail = "old@example.com",
            SenderName = "Old Name",
        });
        await dbContext.SaveChangesAsync();

        EmailProviderConfigRepository repository = new(dbContext);
        EmailProviderConfig config = new(existingId, "new-key", "new@example.com", "New Name");

        // Act
        EmailProviderConfig result = await repository.SaveAsync(config, CancellationToken.None);

        // Assert
        result.Should().Be(config);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
