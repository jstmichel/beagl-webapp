// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Breeds;
using Beagl.Infrastructure.Breeds;
using Beagl.Infrastructure.Breeds.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Beagl.Infrastructure.Tests.Breeds;

public class BreedEntityTests
{
    [Fact]
    public void DefaultValues_ShouldBeEmpty()
    {
        // Act
        BreedEntity entity = new();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
        entity.AnimalType.Should().Be(AnimalType.None);
        entity.NameEn.Should().Be(string.Empty);
        entity.NameFr.Should().Be(string.Empty);
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        BreedEntity entity = new()
        {
            Id = id,
            AnimalType = AnimalType.Cat,
            NameEn = "Maine Coon",
            NameFr = "Maine Coon",
            IsActive = true,
        };

        // Assert
        entity.Id.Should().Be(id);
        entity.AnimalType.Should().Be(AnimalType.Cat);
        entity.NameEn.Should().Be("Maine Coon");
        entity.NameFr.Should().Be("Maine Coon");
        entity.IsActive.Should().BeTrue();
    }
}

public class BreedRepositoryTests
{
    [Fact]
    public void Constructor_WithNullDbContext_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new BreedRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.Breeds.AddRange(
            new BreedEntity { Id = Guid.NewGuid(), AnimalType = AnimalType.Dog, NameEn = "Poodle", NameFr = "Caniche", IsActive = true },
            new BreedEntity { Id = Guid.NewGuid(), AnimalType = AnimalType.Dog, NameEn = "Labrador", NameFr = "Labrador", IsActive = true },
            new BreedEntity { Id = Guid.NewGuid(), AnimalType = AnimalType.Dog, NameEn = "Beagle", NameFr = "Beagle", IsActive = true });

        await dbContext.SaveChangesAsync();

        BreedRepository repository = new(dbContext);
        GetBreedsPageQuery query = new(1, 10, null, null);

        // Act
        BreedsPage result = await repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.Items[0].NameEn.Should().Be("Beagle");
        result.Items[1].NameEn.Should().Be("Labrador");
        result.Items[2].NameEn.Should().Be("Poodle");
    }

    [Fact]
    public async Task GetPageAsync_WithAnimalTypeFilter_ShouldFilterResults()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.Breeds.AddRange(
            new BreedEntity { Id = Guid.NewGuid(), AnimalType = AnimalType.Cat, NameEn = "Maine Coon", NameFr = "Maine Coon", IsActive = true },
            new BreedEntity { Id = Guid.NewGuid(), AnimalType = AnimalType.Dog, NameEn = "Labrador", NameFr = "Labrador", IsActive = true });

        await dbContext.SaveChangesAsync();

        BreedRepository repository = new(dbContext);
        GetBreedsPageQuery query = new(1, 10, AnimalType.Cat, null);

        // Act
        BreedsPage result = await repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].NameEn.Should().Be("Maine Coon");
        result.Items[0].AnimalType.Should().Be(AnimalType.Cat);
    }

    [Fact]
    public async Task GetPageAsync_WithIsActiveFilter_ShouldFilterResults()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.Breeds.AddRange(
            new BreedEntity { Id = Guid.NewGuid(), AnimalType = AnimalType.Cat, NameEn = "Maine Coon", NameFr = "Maine Coon", IsActive = true },
            new BreedEntity { Id = Guid.NewGuid(), AnimalType = AnimalType.Cat, NameEn = "Persian", NameFr = "Persan", IsActive = false });

        await dbContext.SaveChangesAsync();

        BreedRepository repository = new(dbContext);
        GetBreedsPageQuery query = new(1, 10, null, true);

        // Act
        BreedsPage result = await repository.GetPageAsync(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].NameEn.Should().Be("Maine Coon");
        result.Items[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnBreed()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.Breeds.Add(new BreedEntity
        {
            Id = id,
            AnimalType = AnimalType.Dog,
            NameEn = "Labrador",
            NameFr = "Labrador",
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();

        BreedRepository repository = new(dbContext);

        // Act
        Breed? result = await repository.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.AnimalType.Should().Be(AnimalType.Dog);
        result.NameEn.Should().Be("Labrador");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        BreedRepository repository = new(dbContext);

        // Act
        Breed? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenDuplicateExists_ShouldReturnTrue()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.Breeds.Add(new BreedEntity
        {
            Id = Guid.NewGuid(),
            AnimalType = AnimalType.Cat,
            NameEn = "Maine Coon",
            NameFr = "Maine Coon",
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();

        BreedRepository repository = new(dbContext);

        // Act
        bool result = await repository.ExistsAsync(AnimalType.Cat, "Maine Coon", "Maine Coon", null, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenNoDuplicate_ShouldReturnFalse()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        BreedRepository repository = new(dbContext);

        // Act
        bool result = await repository.ExistsAsync(AnimalType.Dog, "Labrador", "Labrador", null, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExcludeId_ShouldExcludeSelf()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.Breeds.Add(new BreedEntity
        {
            Id = id,
            AnimalType = AnimalType.Cat,
            NameEn = "Maine Coon",
            NameFr = "Maine Coon",
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();

        BreedRepository repository = new(dbContext);

        // Act
        bool result = await repository.ExistsAsync(AnimalType.Cat, "Maine Coon", "Maine Coon", id, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPageAsync_WithNullQuery_ShouldThrowArgumentNullException()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        BreedRepository repository = new(dbContext);

        // Act
        Func<Task> act = async () => await repository.GetPageAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_WithNullBreed_ShouldThrowArgumentNullException()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        BreedRepository repository = new(dbContext);

        // Act
        Func<Task> act = async () => await repository.CreateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertAndReturnBreed()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        BreedRepository repository = new(dbContext);

        Guid id = Guid.NewGuid();
        Breed breed = new(id, AnimalType.Cat, "Maine Coon", "Maine Coon", true);

        // Act
        Breed created = await repository.CreateAsync(breed, CancellationToken.None);

        // Assert
        created.Id.Should().Be(id);
        created.AnimalType.Should().Be(AnimalType.Cat);
        created.NameEn.Should().Be("Maine Coon");
        created.NameFr.Should().Be("Maine Coon");
        created.IsActive.Should().BeTrue();

        int rowCount = await dbContext.Breeds.CountAsync();
        rowCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_WithNullBreed_ShouldThrowArgumentNullException()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        BreedRepository repository = new(dbContext);

        // Act
        Func<Task> act = async () => await repository.UpdateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenBreedNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await using ApplicationDbContext dbContext = CreateDbContext();
        BreedRepository repository = new(dbContext);
        Breed breed = new(Guid.NewGuid(), AnimalType.Cat, "Maine Coon", "Maine Coon", true);

        // Act
        Func<Task> act = async () => await repository.UpdateAsync(breed, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyAndReturnBreed()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        await using ApplicationDbContext dbContext = CreateDbContext();

        dbContext.Breeds.Add(new BreedEntity
        {
            Id = id,
            AnimalType = AnimalType.Cat,
            NameEn = "Maine Coon",
            NameFr = "Maine Coon",
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();

        BreedRepository repository = new(dbContext);
        Breed updated = new(id, AnimalType.Dog, "Labrador", "Labrador", false);

        // Act
        Breed result = await repository.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Id.Should().Be(id);
        result.AnimalType.Should().Be(AnimalType.Dog);
        result.NameEn.Should().Be("Labrador");
        result.NameFr.Should().Be("Labrador");
        result.IsActive.Should().BeFalse();

        BreedEntity? persisted = await dbContext.Breeds.FindAsync(id);
        persisted!.NameEn.Should().Be("Labrador");
        persisted.IsActive.Should().BeFalse();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
