// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Breeds.Dtos;
using Beagl.Application.Breeds.Services;
using Beagl.Domain.Breeds;
using Beagl.Domain.Results;
using FluentAssertions;
using Moq;

namespace Beagl.Application.Tests.Breeds.Services;

public class BreedServiceTests
{
    private readonly Mock<IBreedRepository> _repositoryMock = new();

    private BreedService CreateService() => new(_repositoryMock.Object);

    public static IEnumerable<object[]> InvalidInputCases()
    {
        yield return new object[] { 99, "Name", "Nom", string.Empty, string.Empty, "breed.animal_type_required" };
        yield return new object[] { 1, string.Empty, "Nom", string.Empty, string.Empty, "breed.name_en_required" };
        yield return new object[] { 1, "   ", "Nom", string.Empty, string.Empty, "breed.name_en_required" };
        yield return new object[] { 1, new string('a', 101), "Nom", string.Empty, string.Empty, "breed.name_en_too_long" };
        yield return new object[] { 1, "Name", string.Empty, string.Empty, string.Empty, "breed.name_fr_required" };
        yield return new object[] { 1, "Name", "   ", string.Empty, string.Empty, "breed.name_fr_required" };
        yield return new object[] { 1, "Name", new string('a', 101), string.Empty, string.Empty, "breed.name_fr_too_long" };
        yield return new object[] { 1, "Name", "Nom", new string('a', 501), string.Empty, "breed.description_en_too_long" };
        yield return new object[] { 1, "Name", "Nom", string.Empty, new string('a', 501), "breed.description_fr_too_long" };
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new BreedService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPageAsync_ShouldDelegateToRepository_AndReturnMappedDto()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Breed breed = new(id, AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty, true);
        BreedsPage page = new([breed], 1);

        _repositoryMock
            .Setup(r => r.GetPageAsync(It.IsAny<GetBreedsPageQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        BreedService service = CreateService();

        // Act
        BreedsPageDto result = await service.GetPageAsync(1, 10, null, null, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(id);
        result.Items[0].AnimalType.Should().Be(AnimalType.Cat);
        result.Items[0].NameEn.Should().Be("Maine Coon");
    }

    [Fact]
    public async Task GetPageAsync_WithFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        GetBreedsPageQuery? capturedQuery = null;

        _repositoryMock
            .Setup(r => r.GetPageAsync(It.IsAny<GetBreedsPageQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetBreedsPageQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new BreedsPage([], 0));

        BreedService service = CreateService();

        // Act
        await service.GetPageAsync(2, 5, AnimalType.Dog, false, CancellationToken.None);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.Page.Should().Be(2);
        capturedQuery.PageSize.Should().Be(5);
        capturedQuery.AnimalTypeFilter.Should().Be(AnimalType.Dog);
        capturedQuery.IsActiveFilter.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WhenBreedExists_ShouldReturnMappedDto()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Breed breed = new(id, AnimalType.Dog, "Labrador", "Labrador", "Friendly dog", "Chien amical", true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(breed);

        BreedService service = CreateService();

        // Act
        BreedDto? result = await service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.AnimalType.Should().Be(AnimalType.Dog);
        result.NameEn.Should().Be("Labrador");
        result.DescriptionEn.Should().Be("Friendly dog");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WhenBreedNotFound_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Breed?)null);

        BreedService service = CreateService();

        // Act
        BreedDto? result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        BreedService service = CreateService();

        // Act
        Func<Task> act = async () => await service.CreateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        Breed? capturedBreed = null;

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<AnimalType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()))
            .Callback<Breed, CancellationToken>((b, _) => capturedBreed = b)
            .ReturnsAsync((Breed b, CancellationToken _) => b);

        BreedService service = CreateService();
        SaveBreedRequest request = new(AnimalType.Cat, "Maine Coon", "Maine Coon", "A large cat", "Un grand chat");

        // Act
        Result<BreedDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.NameEn.Should().Be("Maine Coon");
        result.Value.AnimalType.Should().Be(AnimalType.Cat);
        capturedBreed.Should().NotBeNull();
        capturedBreed!.Id.Should().NotBe(Guid.Empty);
        capturedBreed.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimInputValues()
    {
        // Arrange
        Breed? capturedBreed = null;

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<AnimalType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()))
            .Callback<Breed, CancellationToken>((b, _) => capturedBreed = b)
            .ReturnsAsync((Breed b, CancellationToken _) => b);

        BreedService service = CreateService();
        SaveBreedRequest request = new(AnimalType.Cat, "  Maine Coon  ", "  Maine Coon  ", "  A large cat  ", "  Un grand chat  ");

        // Act
        await service.CreateAsync(request, CancellationToken.None);

        // Assert
        capturedBreed!.NameEn.Should().Be("Maine Coon");
        capturedBreed.NameFr.Should().Be("Maine Coon");
        capturedBreed.DescriptionEn.Should().Be("A large cat");
        capturedBreed.DescriptionFr.Should().Be("Un grand chat");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<AnimalType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        BreedService service = CreateService();
        SaveBreedRequest request = new(AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty);

        // Act
        Result<BreedDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("breed.duplicate_name");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(InvalidInputCases))]
    public async Task CreateAsync_WithInvalidInput_ShouldReturnValidationFailure(
        int animalTypeValue,
        string nameEn,
        string nameFr,
        string descriptionEn,
        string descriptionFr,
        string expectedCode)
    {
        // Arrange
        BreedService service = CreateService();
        SaveBreedRequest request = new((AnimalType)animalTypeValue, nameEn, nameFr, descriptionEn, descriptionFr);

        // Act
        Result<BreedDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(expectedCode);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        BreedService service = CreateService();

        // Act
        Func<Task> act = async () => await service.UpdateAsync(Guid.NewGuid(), null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenBreedNotFound_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Breed?)null);

        BreedService service = CreateService();
        SaveBreedRequest request = new(AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty);

        // Act
        Result<BreedDto> result = await service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("breed.not_found");
    }

    [Fact]
    public async Task UpdateAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Breed existing = new(id, AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty, true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<AnimalType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Breed b, CancellationToken _) => b);

        BreedService service = CreateService();
        SaveBreedRequest request = new(AnimalType.Dog, "Golden Retriever", "Golden Retriever", "Playful", "Joueur");

        // Act
        Result<BreedDto> result = await service.UpdateAsync(id, request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.NameEn.Should().Be("Golden Retriever");
        result.Value.AnimalType.Should().Be(AnimalType.Dog);
    }

    [Fact]
    public async Task UpdateAsync_ShouldTrimInputValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Breed existing = new(id, AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty, true);
        Breed? capturedBreed = null;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<AnimalType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()))
            .Callback<Breed, CancellationToken>((b, _) => capturedBreed = b)
            .ReturnsAsync((Breed b, CancellationToken _) => b);

        BreedService service = CreateService();
        SaveBreedRequest request = new(AnimalType.Dog, "  Golden  ", "  Retriever  ", "  Playful  ", "  Joueur  ");

        // Act
        await service.UpdateAsync(id, request, CancellationToken.None);

        // Assert
        capturedBreed!.NameEn.Should().Be("Golden");
        capturedBreed.NameFr.Should().Be("Retriever");
        capturedBreed.DescriptionEn.Should().Be("Playful");
        capturedBreed.DescriptionFr.Should().Be("Joueur");
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Breed existing = new(id, AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty, true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<AnimalType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        BreedService service = CreateService();
        SaveBreedRequest request = new(AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty);

        // Act
        Result<BreedDto> result = await service.UpdateAsync(id, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("breed.duplicate_name");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(InvalidInputCases))]
    public async Task UpdateAsync_WithInvalidInput_ShouldReturnValidationFailure(
        int animalTypeValue,
        string nameEn,
        string nameFr,
        string descriptionEn,
        string descriptionFr,
        string expectedCode)
    {
        // Arrange
        BreedService service = CreateService();
        SaveBreedRequest request = new((AnimalType)animalTypeValue, nameEn, nameFr, descriptionEn, descriptionFr);

        // Act
        Result<BreedDto> result = await service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(expectedCode);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenBreedExists_ShouldToggleIsActive()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Breed existing = new(id, AnimalType.Cat, "Maine Coon", "Maine Coon", string.Empty, string.Empty, true);
        Breed? capturedBreed = null;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()))
            .Callback<Breed, CancellationToken>((b, _) => capturedBreed = b)
            .ReturnsAsync((Breed b, CancellationToken _) => b);

        BreedService service = CreateService();

        // Act
        Result result = await service.ToggleActiveAsync(id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedBreed!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenBreedNotFound_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Breed?)null);

        BreedService service = CreateService();

        // Act
        Result result = await service.ToggleActiveAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("breed.not_found");
    }
}
