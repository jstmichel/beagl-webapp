// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Reflection;
using Beagl.Application.Breeds.Dtos;
using Beagl.Application.Breeds.Services;
using Beagl.Domain.Breeds;
using Beagl.Domain.Results;
using Beagl.WebApp.Components.Admin;
using Beagl.WebApp.Resources;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Beagl.WebApp.Tests.Components.Admin;

public sealed class BreedsTabTests
{
    private readonly Mock<IBreedService> _serviceMock = new();
    private readonly Mock<IStringLocalizer<BreedsResource>> _localizerMock = new();
    private readonly Mock<ILogger<BreedsTab>> _loggerMock = new();

    private BreedsTab CreateComponent()
    {
        BreedsTab component = new();

        typeof(BreedsTab)
            .GetProperty("BreedService", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(component, _serviceMock.Object);

        typeof(BreedsTab)
            .GetProperty("L", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(component, _localizerMock.Object);

        typeof(BreedsTab)
            .GetProperty("Logger", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(component, _loggerMock.Object);

        // Default: return the key as the localized value so callers never receive null.
        _localizerMock.As<IStringLocalizer>()
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        return component;
    }

    private static T GetField<T>(BreedsTab component, string fieldName) where T : notnull
    {
        return (T)typeof(BreedsTab)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(component)!;
    }

    private static object? GetRawField(BreedsTab component, string fieldName)
    {
        return typeof(BreedsTab)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(component);
    }

    private static void SetField(BreedsTab component, string fieldName, object? value)
    {
        typeof(BreedsTab)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(component, value);
    }

    private static async Task InvokePrivateMethodAsync(BreedsTab component, string methodName, object?[]? args = null)
    {
        Task task = (Task)typeof(BreedsTab)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, args)!;

        await task;
    }

    private static void InvokePrivateMethod(BreedsTab component, string methodName, object?[]? args = null)
    {
        typeof(BreedsTab)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, args);
    }

    private static void SetFormField(BreedsTab component, string propertyName, object value)
    {
        object form = typeof(BreedsTab)
            .GetField("_form", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(component)!;

        form.GetType()
            .GetProperty(propertyName)!
            .SetValue(form, value);
    }

    private static T GetFormField<T>(BreedsTab component, string propertyName) where T : notnull
    {
        object form = typeof(BreedsTab)
            .GetField("_form", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(component)!;

        return (T)form.GetType()
            .GetProperty(propertyName)!
            .GetValue(form)!;
    }

    private void SetupGetPageAsync(BreedsPageDto? response = null)
    {
        _serviceMock
            .Setup(s => s.GetPageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AnimalType?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response ?? new BreedsPageDto([], 0));
    }

    [Fact]
    public async Task LoadBreedsAsync_WhenServiceReturnsResults_SetsBreedsList()
    {
        // Arrange
        BreedDto breed = new(Guid.NewGuid(), AnimalType.Cat, "Maine Coon", "Maine Coon", true);
        SetupGetPageAsync(new BreedsPageDto([breed], 1));

        BreedsTab component = CreateComponent();

        // Act
        await InvokePrivateMethodAsync(component, "LoadBreedsAsync");

        // Assert
        List<BreedDto> breeds = GetField<List<BreedDto>>(component, "_breeds");
        breeds.Should().ContainSingle();
        breeds[0].Id.Should().Be(breed.Id);
        breeds[0].NameEn.Should().Be("Maine Coon");

        int totalCount = (int)GetRawField(component, "_totalCount")!;
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task LoadBreedsAsync_WhenServiceThrows_SetsErrorMessage()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetPageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AnimalType?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        BreedsTab component = CreateComponent();

        // Act
        await InvokePrivateMethodAsync(component, "LoadBreedsAsync");

        // Assert
        string? errorMessage = (string?)GetRawField(component, "_errorMessage");
        errorMessage.Should().Be("Breeds.Error.LoadFailed");
    }

    [Fact]
    public async Task OnFilterChangedAsync_ResetsPageAndReloads()
    {
        // Arrange
        SetupGetPageAsync();
        BreedsTab component = CreateComponent();
        SetField(component, "_currentPage", 3);

        // Act
        await InvokePrivateMethodAsync(component, "OnFilterChangedAsync");

        // Assert
        int currentPage = (int)GetRawField(component, "_currentPage")!;
        currentPage.Should().Be(1);

        _serviceMock.Verify(
            s => s.GetPageAsync(1, It.IsAny<int>(), It.IsAny<AnimalType?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GoToPreviousPageAsync_DecreasesPage()
    {
        // Arrange
        SetupGetPageAsync();
        BreedsTab component = CreateComponent();
        SetField(component, "_currentPage", 2);

        // Act
        await InvokePrivateMethodAsync(component, "GoToPreviousPageAsync");

        // Assert
        int currentPage = (int)GetRawField(component, "_currentPage")!;
        currentPage.Should().Be(1);
    }

    [Fact]
    public async Task GoToPreviousPageAsync_WhenOnFirstPage_DoesNothing()
    {
        // Arrange
        BreedsTab component = CreateComponent();

        // Act
        await InvokePrivateMethodAsync(component, "GoToPreviousPageAsync");

        // Assert
        int currentPage = (int)GetRawField(component, "_currentPage")!;
        currentPage.Should().Be(1);

        _serviceMock.Verify(
            s => s.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AnimalType?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GoToNextPageAsync_IncreasesPage()
    {
        // Arrange
        SetupGetPageAsync();
        BreedsTab component = CreateComponent();
        SetField(component, "_totalCount", 15);

        // Act
        await InvokePrivateMethodAsync(component, "GoToNextPageAsync");

        // Assert
        int currentPage = (int)GetRawField(component, "_currentPage")!;
        currentPage.Should().Be(2);
    }

    [Fact]
    public async Task GoToNextPageAsync_WhenOnLastPage_DoesNothing()
    {
        // Arrange
        BreedsTab component = CreateComponent();

        // Act
        await InvokePrivateMethodAsync(component, "GoToNextPageAsync");

        // Assert
        int currentPage = (int)GetRawField(component, "_currentPage")!;
        currentPage.Should().Be(1);

        _serviceMock.Verify(
            s => s.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AnimalType?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void OpenCreateModal_ResetsFormAndShowsModal()
    {
        // Arrange
        BreedsTab component = CreateComponent();
        SetFormField(component, "NameEn", "Previous Value");

        // Act
        InvokePrivateMethod(component, "OpenCreateModal");

        // Assert
        bool showModal = (bool)GetRawField(component, "_showModal")!;
        showModal.Should().BeTrue();

        object? editingBreedId = GetRawField(component, "_editingBreedId");
        editingBreedId.Should().BeNull();

        string nameEn = GetFormField<string>(component, "NameEn");
        nameEn.Should().Be(string.Empty);
    }

    [Fact]
    public void OpenEditModal_PopulatesFormAndShowsModal()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        BreedDto breed = new(id, AnimalType.Dog, "Labrador", "Labrador", true);
        BreedsTab component = CreateComponent();

        // Act
        InvokePrivateMethod(component, "OpenEditModal", new object[] { breed });

        // Assert
        bool showModal = (bool)GetRawField(component, "_showModal")!;
        showModal.Should().BeTrue();

        Guid? editingBreedId = (Guid?)GetRawField(component, "_editingBreedId");
        editingBreedId.Should().Be(id);

        AnimalType animalType = GetFormField<AnimalType>(component, "AnimalType");
        animalType.Should().Be(AnimalType.Dog);

        string nameEn = GetFormField<string>(component, "NameEn");
        nameEn.Should().Be("Labrador");
    }

    [Fact]
    public void CloseModal_HidesModalAndClearsErrors()
    {
        // Arrange
        BreedsTab component = CreateComponent();
        SetField(component, "_showModal", true);

        List<string> formErrors = GetField<List<string>>(component, "_formErrors");
        formErrors.Add("some error");

        // Act
        InvokePrivateMethod(component, "CloseModal");

        // Assert
        bool showModal = (bool)GetRawField(component, "_showModal")!;
        showModal.Should().BeFalse();
        formErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveBreedAsync_Create_OnSuccess_ClosesModalAndReloads()
    {
        // Arrange
        BreedDto created = new(Guid.NewGuid(), AnimalType.Cat, "Maine Coon", "Maine Coon", true);

        _serviceMock
            .Setup(s => s.CreateAsync(It.IsAny<SaveBreedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(created));

        SetupGetPageAsync();
        BreedsTab component = CreateComponent();
        SetField(component, "_showModal", true);

        // Act
        await InvokePrivateMethodAsync(component, "SaveBreedAsync");

        // Assert
        bool showModal = (bool)GetRawField(component, "_showModal")!;
        showModal.Should().BeFalse();

        string? statusMessage = (string?)GetRawField(component, "_statusMessage");
        statusMessage.Should().Be("Breeds.Success.Created");
    }

    [Fact]
    public async Task SaveBreedAsync_Create_OnFailure_AddsLocalizedErrorToFormErrors()
    {
        // Arrange
        ResultError error = new("breed.duplicate_name", "A breed with the same animal type and name already exists.");

        _serviceMock
            .Setup(s => s.CreateAsync(It.IsAny<SaveBreedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<BreedDto>(error));

        BreedsTab component = CreateComponent();
        SetField(component, "_showModal", true);

        // Act
        await InvokePrivateMethodAsync(component, "SaveBreedAsync");

        // Assert
        List<string> formErrors = GetField<List<string>>(component, "_formErrors");
        formErrors.Should().ContainSingle().Which.Should().Be("breed.duplicate_name");

        bool showModal = (bool)GetRawField(component, "_showModal")!;
        showModal.Should().BeTrue();
    }

    [Fact]
    public async Task SaveBreedAsync_Update_OnSuccess_ClosesModalAndReloads()
    {
        // Arrange
        Guid editingId = Guid.NewGuid();
        BreedDto updated = new(editingId, AnimalType.Dog, "Labrador", "Labrador", true);

        _serviceMock
            .Setup(s => s.UpdateAsync(editingId, It.IsAny<SaveBreedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(updated));

        SetupGetPageAsync();
        BreedsTab component = CreateComponent();
        SetField(component, "_editingBreedId", (Guid?)editingId);
        SetField(component, "_showModal", true);

        // Act
        await InvokePrivateMethodAsync(component, "SaveBreedAsync");

        // Assert
        bool showModal = (bool)GetRawField(component, "_showModal")!;
        showModal.Should().BeFalse();

        string? statusMessage = (string?)GetRawField(component, "_statusMessage");
        statusMessage.Should().Be("Breeds.Success.Updated");
    }

    [Fact]
    public async Task SaveBreedAsync_Update_OnFailure_AddsLocalizedErrorToFormErrors()
    {
        // Arrange
        Guid editingId = Guid.NewGuid();
        ResultError error = new("breed.not_found", "The breed was not found.");

        _serviceMock
            .Setup(s => s.UpdateAsync(editingId, It.IsAny<SaveBreedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<BreedDto>(error));

        BreedsTab component = CreateComponent();
        SetField(component, "_editingBreedId", (Guid?)editingId);
        SetField(component, "_showModal", true);

        // Act
        await InvokePrivateMethodAsync(component, "SaveBreedAsync");

        // Assert
        List<string> formErrors = GetField<List<string>>(component, "_formErrors");
        formErrors.Should().ContainSingle().Which.Should().Be("breed.not_found");
    }

    [Fact]
    public async Task ToggleActiveAsync_OnSuccess_SetsStatusMessageAndReloads()
    {
        // Arrange
        Guid breedId = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.ToggleActiveAsync(breedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        SetupGetPageAsync();
        BreedsTab component = CreateComponent();

        // Act
        await InvokePrivateMethodAsync(component, "ToggleActiveAsync", new object[] { breedId });

        // Assert
        string? statusMessage = (string?)GetRawField(component, "_statusMessage");
        statusMessage.Should().Be("Breeds.Success.StatusChanged");

        _serviceMock.Verify(
            s => s.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AnimalType?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ToggleActiveAsync_OnFailure_SetsErrorMessage()
    {
        // Arrange
        Guid breedId = Guid.NewGuid();
        ResultError error = new("breed.not_found", "The breed was not found.");

        _serviceMock
            .Setup(s => s.ToggleActiveAsync(breedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        BreedsTab component = CreateComponent();

        // Act
        await InvokePrivateMethodAsync(component, "ToggleActiveAsync", new object[] { breedId });

        // Assert
        string? errorMessage = (string?)GetRawField(component, "_errorMessage");
        errorMessage.Should().Be("breed.not_found");
    }

    [Fact]
    public void Dispose_CancelsTokenSource()
    {
        // Arrange
        BreedsTab component = CreateComponent();
        CancellationTokenSource cts = GetField<CancellationTokenSource>(component, "_cts");

        // Act
        component.Dispose();

        // Assert
        cts.IsCancellationRequested.Should().BeTrue();
    }
}
