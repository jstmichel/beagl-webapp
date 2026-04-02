// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.Infrastructure.Database;
using Beagl.Infrastructure.Users;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Tests.Users;

public sealed class CitizenProfileRepositoryTests
{
    private static readonly Address ValidAddress = new("123 Main St", "Montreal", "Quebec", "H1A 1A1");

    [Fact]
    public void Constructor_WithNullDbContext_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new CitizenProfileRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenProfileExists_ShouldReturnMappedDomain()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        Guid profileId = Guid.NewGuid();
        context.CitizenProfiles.Add(new CitizenProfileEntity
        {
            Id = profileId,
            UserId = "user-1",
            FirstName = "Jane",
            LastName = "Doe",
            Address = new AddressEntity
            {
                Street = "123 Main St",
                City = "Montreal",
                Province = "Quebec",
                PostalCode = "H1A 1A1",
            },
            DateOfBirth = new DateOnly(1990, 5, 15),
            CommunicationPreference = CommunicationPreference.Email,
            LanguagePreference = LanguagePreference.English,
        });
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        CitizenProfile? result = await repository.GetByUserIdAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profileId);
        result.UserId.Should().Be("user-1");
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
        result.Address.Should().NotBeNull();
        result.Address!.Street.Should().Be("123 Main St");
        result.Address.City.Should().Be("Montreal");
        result.Address.Province.Should().Be("Quebec");
        result.Address.PostalCode.Should().Be("H1A 1A1");
        result.DateOfBirth.Should().Be(new DateOnly(1990, 5, 15));
        result.CommunicationPreference.Should().Be(CommunicationPreference.Email);
        result.LanguagePreference.Should().Be(LanguagePreference.English);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenProfileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        CitizenProfileRepository repository = new(context);

        // Act
        CitizenProfile? result = await repository.GetByUserIdAsync("missing-user", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenProfileHasNullOptionalFields_ShouldMapNulls()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.CitizenProfiles.Add(new CitizenProfileEntity
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            FirstName = "Jane",
            LastName = "Doe",
            Address = null,
            DateOfBirth = null,
            CommunicationPreference = CommunicationPreference.None,
            LanguagePreference = LanguagePreference.None,
        });
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        CitizenProfile? result = await repository.GetByUserIdAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Address.Should().BeNull();
        result.DateOfBirth.Should().BeNull();
        result.CommunicationPreference.Should().Be(CommunicationPreference.None);
        result.LanguagePreference.Should().Be(LanguagePreference.None);
    }

    [Fact]
    public async Task UpdateAsync_WithNullProfile_ShouldThrowArgumentNullException()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        CitizenProfileRepository repository = new(context);

        // Act
        Func<Task> act = async () => await repository.UpdateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenProfileExists_ShouldUpdateAndReturnMappedDomain()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        Guid profileId = Guid.NewGuid();
        context.CitizenProfiles.Add(new CitizenProfileEntity
        {
            Id = profileId,
            UserId = "user-1",
            FirstName = "Jane",
            LastName = "Doe",
            Address = null,
            DateOfBirth = null,
            CommunicationPreference = CommunicationPreference.None,
            LanguagePreference = LanguagePreference.None,
        });
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        UpdateCitizenProfile update = new(
            "user-1",
            "John",
            "Smith",
            new Address("456 Oak Ave", "Toronto", "Ontario", "M5A 1A1"),
            new DateOnly(1985, 3, 20),
            CommunicationPreference.Phone,
            LanguagePreference.French);

        // Act
        Result<CitizenProfile> result = await repository.UpdateAsync(update, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(profileId);
        result.Value.UserId.Should().Be("user-1");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Smith");
        result.Value.Address.Should().NotBeNull();
        result.Value.Address!.Street.Should().Be("456 Oak Ave");
        result.Value.Address.City.Should().Be("Toronto");
        result.Value.Address.Province.Should().Be("Ontario");
        result.Value.Address.PostalCode.Should().Be("M5A 1A1");
        result.Value.DateOfBirth.Should().Be(new DateOnly(1985, 3, 20));
        result.Value.CommunicationPreference.Should().Be(CommunicationPreference.Phone);
        result.Value.LanguagePreference.Should().Be(LanguagePreference.French);
    }

    [Fact]
    public async Task UpdateAsync_WhenProfileExists_ShouldPersistChanges()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        Guid profileId = Guid.NewGuid();
        context.CitizenProfiles.Add(new CitizenProfileEntity
        {
            Id = profileId,
            UserId = "user-1",
            FirstName = "Jane",
            LastName = "Doe",
            Address = null,
            DateOfBirth = null,
            CommunicationPreference = CommunicationPreference.None,
            LanguagePreference = LanguagePreference.None,
        });
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        UpdateCitizenProfile update = new(
            "user-1",
            "John",
            "Smith",
            new Address("456 Oak Ave", "Toronto", "Ontario", "M5A 1A1"),
            new DateOnly(1985, 3, 20),
            CommunicationPreference.Mail,
            LanguagePreference.English);

        // Act
        _ = await repository.UpdateAsync(update, CancellationToken.None);

        // Assert
        CitizenProfileEntity? persisted = await context.CitizenProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == "user-1");

        persisted.Should().NotBeNull();
        persisted!.FirstName.Should().Be("John");
        persisted.LastName.Should().Be("Smith");
        persisted.Address.Should().NotBeNull();
        persisted.Address!.Street.Should().Be("456 Oak Ave");
        persisted.Address.City.Should().Be("Toronto");
        persisted.Address.Province.Should().Be("Ontario");
        persisted.Address.PostalCode.Should().Be("M5A 1A1");
        persisted.DateOfBirth.Should().Be(new DateOnly(1985, 3, 20));
        persisted.CommunicationPreference.Should().Be(CommunicationPreference.Mail);
        persisted.LanguagePreference.Should().Be(LanguagePreference.English);
    }

    [Fact]
    public async Task UpdateAsync_WhenProfileDoesNotExist_ShouldCreateAndReturnProfile()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        CitizenProfileRepository repository = new(context);

        UpdateCitizenProfile update = new(
            "missing-user",
            "John",
            "Doe",
            ValidAddress,
            new DateOnly(1990, 5, 15),
            CommunicationPreference.Email,
            LanguagePreference.English);

        // Act
        Result<CitizenProfile> result = await repository.UpdateAsync(update, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be("missing-user");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Address.Should().Be(ValidAddress);
        result.Value.DateOfBirth.Should().Be(new DateOnly(1990, 5, 15));
    }

    [Fact]
    public async Task UpdateAsync_WhenCancellationRequested_ShouldThrowOperationCancelledException()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        CitizenProfileRepository repository = new(context);
        CancellationTokenSource cts = new();
        await cts.CancelAsync();

        UpdateCitizenProfile update = new(
            "user-1",
            "John",
            "Doe",
            ValidAddress,
            new DateOnly(1990, 5, 15),
            CommunicationPreference.Email,
            LanguagePreference.English);

        // Act
        Func<Task> act = async () => await repository.UpdateAsync(update, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenProfileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("missing-user", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenAllFieldsPopulated_ShouldReturnTrue()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.CitizenProfiles.Add(CreateCompleteEntity("user-1"));
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenFirstNameEmpty_ShouldReturnFalse()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.CitizenProfiles.Add(CreateCompleteEntity("user-1", firstName: ""));
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenLastNameEmpty_ShouldReturnFalse()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.CitizenProfiles.Add(CreateCompleteEntity("user-1", lastName: ""));
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenAddressNull_ShouldReturnFalse()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.CitizenProfiles.Add(new CitizenProfileEntity
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            FirstName = "Jane",
            LastName = "Doe",
            Address = null,
            DateOfBirth = new DateOnly(1990, 5, 15),
            CommunicationPreference = CommunicationPreference.Email,
            LanguagePreference = LanguagePreference.English,
        });
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenDateOfBirthNull_ShouldReturnFalse()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        CitizenProfileEntity entity = CreateCompleteEntity("user-1");
        entity.DateOfBirth = null;
        context.CitizenProfiles.Add(entity);
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenCommunicationPreferenceNone_ShouldReturnFalse()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.CitizenProfiles.Add(
            CreateCompleteEntity("user-1", communicationPreference: CommunicationPreference.None));
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenLanguagePreferenceNone_ShouldReturnFalse()
    {
        // Arrange
        ApplicationDbContext context = CreateDbContext();
        context.CitizenProfiles.Add(
            CreateCompleteEntity("user-1", languagePreference: LanguagePreference.None));
        await context.SaveChangesAsync();

        CitizenProfileRepository repository = new(context);

        // Act
        bool result = await repository.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static CitizenProfileEntity CreateCompleteEntity(
        string userId,
        string firstName = "Jane",
        string lastName = "Doe",
        AddressEntity? address = null,
        CommunicationPreference communicationPreference = CommunicationPreference.Email,
        LanguagePreference languagePreference = LanguagePreference.English)
    {
        return new CitizenProfileEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Address = address ?? new AddressEntity
            {
                Street = "123 Main St",
                City = "Montreal",
                Province = "Quebec",
                PostalCode = "H1A 1A1",
            },
            DateOfBirth = new DateOnly(1990, 5, 15),
            CommunicationPreference = communicationPreference,
            LanguagePreference = languagePreference,
        };
    }
}
