// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Colors.Dtos;
using Beagl.Application.Colors.Services;
using Beagl.Domain.Colors;
using Beagl.Domain.Results;
using FluentAssertions;
using Moq;

namespace Beagl.Application.Tests.Colors.Services;

public class ColorServiceTests
{
    private readonly Mock<IColorRepository> _repositoryMock = new();

    private ColorService CreateService() => new(_repositoryMock.Object);

    public static IEnumerable<object[]> InvalidInputCases()
    {
        yield return new object[] { string.Empty, "Nom", "color.name_en_required" };
        yield return new object[] { "   ", "Nom", "color.name_en_required" };
        yield return new object[] { new string('a', 101), "Nom", "color.name_en_too_long" };
        yield return new object[] { "Name", string.Empty, "color.name_fr_required" };
        yield return new object[] { "Name", "   ", "color.name_fr_required" };
        yield return new object[] { "Name", new string('a', 101), "color.name_fr_too_long" };
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new ColorService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPageAsync_ShouldDelegateToRepository_AndReturnMappedDto()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Color color = new(id, "Black", "Noir");
        ColorsPage page = new([color], 1);

        _repositoryMock
            .Setup(r => r.GetPageAsync(It.IsAny<GetColorsPageQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        ColorService service = CreateService();

        // Act
        ColorsPageDto result = await service.GetPageAsync(1, 10, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(id);
        result.Items[0].NameEn.Should().Be("Black");
        result.Items[0].NameFr.Should().Be("Noir");
    }

    [Fact]
    public async Task GetPageAsync_ShouldPassQueryToRepository()
    {
        // Arrange
        GetColorsPageQuery? capturedQuery = null;

        _repositoryMock
            .Setup(r => r.GetPageAsync(It.IsAny<GetColorsPageQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetColorsPageQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new ColorsPage([], 0));

        ColorService service = CreateService();

        // Act
        await service.GetPageAsync(3, 25, CancellationToken.None);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.Page.Should().Be(3);
        capturedQuery.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task GetByIdAsync_WhenColorExists_ShouldReturnMappedDto()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Color color = new(id, "White", "Blanc");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(color);

        ColorService service = CreateService();

        // Act
        ColorDto? result = await service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.NameEn.Should().Be("White");
        result.NameFr.Should().Be("Blanc");
    }

    [Fact]
    public async Task GetByIdAsync_WhenColorNotFound_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        ColorService service = CreateService();

        // Act
        ColorDto? result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        ColorService service = CreateService();

        // Act
        Func<Task> act = async () => await service.CreateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        Color? capturedColor = null;

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .Callback<Color, CancellationToken>((c, _) => capturedColor = c)
            .ReturnsAsync((Color c, CancellationToken _) => c);

        ColorService service = CreateService();
        SaveColorRequest request = new("Brown", "Brun");

        // Act
        Result<ColorDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.NameEn.Should().Be("Brown");
        result.Value.NameFr.Should().Be("Brun");
        capturedColor.Should().NotBeNull();
        capturedColor!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimInputValues()
    {
        // Arrange
        Color? capturedColor = null;

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .Callback<Color, CancellationToken>((c, _) => capturedColor = c)
            .ReturnsAsync((Color c, CancellationToken _) => c);

        ColorService service = CreateService();
        SaveColorRequest request = new("  Brown  ", "  Brun  ");

        // Act
        await service.CreateAsync(request, CancellationToken.None);

        // Assert
        capturedColor!.NameEn.Should().Be("Brown");
        capturedColor.NameFr.Should().Be("Brun");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        ColorService service = CreateService();
        SaveColorRequest request = new("Black", "Noir");

        // Act
        Result<ColorDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("color.duplicate_name");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(InvalidInputCases))]
    public async Task CreateAsync_WithInvalidInput_ShouldReturnValidationFailure(
        string nameEn,
        string nameFr,
        string expectedCode)
    {
        // Arrange
        ColorService service = CreateService();
        SaveColorRequest request = new(nameEn, nameFr);

        // Act
        Result<ColorDto> result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(expectedCode);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        ColorService service = CreateService();

        // Act
        Func<Task> act = async () => await service.UpdateAsync(Guid.NewGuid(), null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenColorNotFound_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        ColorService service = CreateService();
        SaveColorRequest request = new("Gray", "Gris");

        // Act
        Result<ColorDto> result = await service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("color.not_found");
    }

    [Fact]
    public async Task UpdateAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Color existing = new(id, "Black", "Noir");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color c, CancellationToken _) => c);

        ColorService service = CreateService();
        SaveColorRequest request = new("Jet Black", "Noir jais");

        // Act
        Result<ColorDto> result = await service.UpdateAsync(id, request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.NameEn.Should().Be("Jet Black");
        result.Value.NameFr.Should().Be("Noir jais");
    }

    [Fact]
    public async Task UpdateAsync_ShouldTrimInputValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Color existing = new(id, "Black", "Noir");
        Color? capturedColor = null;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .Callback<Color, CancellationToken>((c, _) => capturedColor = c)
            .ReturnsAsync((Color c, CancellationToken _) => c);

        ColorService service = CreateService();
        SaveColorRequest request = new("  Jet Black  ", "  Noir jais  ");

        // Act
        await service.UpdateAsync(id, request, CancellationToken.None);

        // Assert
        capturedColor!.NameEn.Should().Be("Jet Black");
        capturedColor.NameFr.Should().Be("Noir jais");
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Color existing = new(id, "Black", "Noir");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        ColorService service = CreateService();
        SaveColorRequest request = new("White", "Blanc");

        // Act
        Result<ColorDto> result = await service.UpdateAsync(id, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("color.duplicate_name");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(InvalidInputCases))]
    public async Task UpdateAsync_WithInvalidInput_ShouldReturnValidationFailure(
        string nameEn,
        string nameFr,
        string expectedCode)
    {
        // Arrange
        ColorService service = CreateService();
        SaveColorRequest request = new(nameEn, nameFr);

        // Act
        Result<ColorDto> result = await service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(expectedCode);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenColorNotFound_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        ColorService service = CreateService();

        // Act
        Result result = await service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("color.not_found");
    }

    [Fact]
    public async Task DeleteAsync_WhenColorInUse_ShouldReturnFailure()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Color existing = new(id, "Orange", "Orange");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.IsInUseAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        ColorService service = CreateService();

        // Act
        Result result = await service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("color.in_use");
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenColorExistsAndNotInUse_ShouldReturnSuccess()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Color existing = new(id, "Orange", "Orange");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.IsInUseAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        ColorService service = CreateService();

        // Act
        Result result = await service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
