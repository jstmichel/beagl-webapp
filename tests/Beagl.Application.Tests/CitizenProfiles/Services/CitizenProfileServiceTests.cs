// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.CitizenProfiles.Dtos;
using Beagl.Application.CitizenProfiles.Services;
using Beagl.Application.EmailProviders.Services;
using Beagl.Application.Tests.Utilities;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beagl.Application.Tests.CitizenProfiles.Services;

public class CitizenProfileServiceTests
{
    private static readonly Address ValidAddress = new("123 Main St", "Montreal", "Quebec", "H1A 1A1");

    private static CitizenProfile CreateProfile(
        string firstName = "John",
        string lastName = "Doe",
        Address? address = null,
        DateOnly? dateOfBirth = null,
        CommunicationPreference communicationPreference = CommunicationPreference.Email,
        LanguagePreference languagePreference = LanguagePreference.English)
    {
        return new CitizenProfile(
            Guid.NewGuid(),
            "user-1",
            firstName,
            lastName,
            address ?? ValidAddress,
            dateOfBirth ?? new DateOnly(1990, 5, 15),
            communicationPreference,
            languagePreference);
    }

    private static UpdateCitizenProfileRequest CreateRequest(
        string userId = "user-1",
        string firstName = "John",
        string lastName = "Doe",
        Address? address = null,
        DateOnly? dateOfBirth = null,
        CommunicationPreference communicationPreference = CommunicationPreference.Email,
        LanguagePreference languagePreference = LanguagePreference.English)
    {
        return new UpdateCitizenProfileRequest(
            userId,
            firstName,
            lastName,
            address ?? ValidAddress,
            dateOfBirth ?? new DateOnly(1990, 5, 15),
            communicationPreference,
            languagePreference);
    }

    [Fact]
    public async Task GetProfileAsync_WithNullUserId_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Result<CitizenProfileDto> result = await service.GetProfileAsync(null!, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.invalid_user_id");
    }

    [Fact]
    public async Task GetProfileAsync_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Result<CitizenProfileDto> result = await service.GetProfileAsync("   ", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.invalid_user_id");
    }

    [Fact]
    public async Task GetProfileAsync_WhenProfileNotFound_ShouldReturnSkeletonDtoWithIdentityFields()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CitizenProfile?)null);

        UserAccount user = new("user-1", "john", "john@example.com", "514-555-0000", true, false, UserRole.Citizen);
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Result<CitizenProfileDto> result = await service.GetProfileAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().BeEmpty();
        result.Value.LastName.Should().BeEmpty();
        result.Value.IsComplete.Should().BeFalse();
        result.Value.Email.Should().Be("john@example.com");
        result.Value.PhoneNumber.Should().Be("514-555-0000");
        result.Value.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileAsync_WhenProfileExists_ShouldReturnMappedDto()
    {
        // Arrange
        CitizenProfile profile = CreateProfile();

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Result<CitizenProfileDto> result = await service.GetProfileAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Address.Should().Be(ValidAddress);
        result.Value.DateOfBirth.Should().Be(new DateOnly(1990, 5, 15));
        result.Value.CommunicationPreference.Should().Be(CommunicationPreference.Email);
        result.Value.LanguagePreference.Should().Be(LanguagePreference.English);
        result.Value.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileAsync_WhenProfileIsIncomplete_ShouldReturnIsCompleteFalse()
    {
        // Arrange
        CitizenProfile profile = CreateProfile(
            address: null!,
            dateOfBirth: null,
            communicationPreference: CommunicationPreference.None,
            languagePreference: LanguagePreference.None);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Result<CitizenProfileDto> result = await service.GetProfileAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task GetProfileAsync_ShouldTrimUserId()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CitizenProfile?)null);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        _ = await service.GetProfileAsync("  user-1  ", CancellationToken.None);

        // Assert
        repositoryMock.Verify(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfileAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", token))
            .ReturnsAsync((CitizenProfile?)null);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        _ = await service.GetProfileAsync("user-1", token);

        // Assert
        repositoryMock.Verify(r => r.GetByUserIdAsync("user-1", token), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Func<Task> act = async () => await service.UpdateProfileAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest(userId: "  ");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.invalid_user_id");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyFirstName_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest(firstName: "");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.first_name_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithTooLongFirstName_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        string longFirstName = new('A', ValidationConstants.FirstNameMaxLength + 1);
        UpdateCitizenProfileRequest request = CreateRequest(firstName: longFirstName);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.first_name_too_long");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyLastName_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest(lastName: "");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.last_name_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithTooLongLastName_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        string longLastName = new('B', ValidationConstants.LastNameMaxLength + 1);
        UpdateCitizenProfileRequest request = CreateRequest(lastName: longLastName);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.last_name_too_long");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNullAddress_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = new(
            "user-1",
            "John",
            "Doe",
            null!,
            new DateOnly(1990, 5, 15),
            CommunicationPreference.Email,
            LanguagePreference.English);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.address_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyStreet_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Address address = ValidAddress with { Street = "" };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.street_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithTooLongStreet_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        string longStreet = new('S', ValidationConstants.StreetMaxLength + 1);
        Address address = ValidAddress with { Street = longStreet };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.street_too_long");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyCity_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Address address = ValidAddress with { City = "" };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.city_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithTooLongCity_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        string longCity = new('C', ValidationConstants.CityMaxLength + 1);
        Address address = ValidAddress with { City = longCity };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.city_too_long");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyProvince_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Address address = ValidAddress with { Province = "" };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.province_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithTooLongProvince_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        string longProvince = new('P', ValidationConstants.ProvinceMaxLength + 1);
        Address address = ValidAddress with { Province = longProvince };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.province_too_long");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyPostalCode_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Address address = ValidAddress with { PostalCode = "" };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.postal_code_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithTooLongPostalCode_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        string longPostalCode = new('Z', ValidationConstants.PostalCodeMaxLength + 1);
        Address address = ValidAddress with { PostalCode = longPostalCode };
        UpdateCitizenProfileRequest request = CreateRequest(address: address);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.postal_code_too_long");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithDefaultDateOfBirth_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = new(
            "user-1",
            "John",
            "Doe",
            ValidAddress,
            default,
            CommunicationPreference.Email,
            LanguagePreference.English);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.date_of_birth_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNoneCommunicationPreference_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest(communicationPreference: CommunicationPreference.None);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.communication_preference_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithInvalidCommunicationPreference_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest(communicationPreference: (CommunicationPreference)999);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.communication_preference_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNoneLanguagePreference_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest(languagePreference: LanguagePreference.None);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.language_preference_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithInvalidLanguagePreference_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest(languagePreference: (LanguagePreference)999);

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.language_preference_required");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_ShouldReturnUpdatedProfile()
    {
        // Arrange
        CitizenProfile updatedProfile = CreateProfile();

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UpdateCitizenProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(updatedProfile));

        FakeLogger<CitizenProfileService> logger = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, logger);

        UpdateCitizenProfileRequest request = CreateRequest();

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Address.Should().Be(ValidAddress);
        result.Value.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_ShouldTrimInputValues()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UpdateCitizenProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(CreateProfile()));

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Address paddedAddress = new("  123 Main St  ", "  Montreal  ", "  Quebec  ", "  H1A 1A1  ");
        UpdateCitizenProfileRequest request = new(
            "  user-1  ",
            "  John  ",
            "  Doe  ",
            paddedAddress,
            new DateOnly(1990, 5, 15),
            CommunicationPreference.Email,
            LanguagePreference.English);

        // Act
        _ = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        repositoryMock.Verify(r => r.UpdateAsync(
            It.Is<UpdateCitizenProfile>(p =>
                p.UserId == "user-1"
                && p.FirstName == "John"
                && p.LastName == "Doe"
                && p.Address.Street == "123 Main St"
                && p.Address.City == "Montreal"
                && p.Address.Province == "Quebec"
                && p.Address.PostalCode == "H1A 1A1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenRepositoryFails_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UpdateCitizenProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CitizenProfile>(
                new ResultError("citizen_profile.not_found", "The citizen profile could not be found.")));

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest();

        // Act
        Result<CitizenProfileDto> result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.not_found");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_ShouldLogProfileUpdated()
    {
        // Arrange
        CitizenProfile updatedProfile = CreateProfile();

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UpdateCitizenProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(updatedProfile));

        FakeLogger<CitizenProfileService> logger = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, logger);

        UpdateCitizenProfileRequest request = CreateRequest();

        // Act
        _ = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        logger.Logs.Should().ContainSingle(log => log.EventId.Id == 2001);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UpdateCitizenProfile>(), token))
            .ReturnsAsync(Result.Success(CreateProfile()));

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenProfileRequest request = CreateRequest();

        // Act
        _ = await service.UpdateProfileAsync(request, token);

        // Assert
        repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UpdateCitizenProfile>(), token), Times.Once);
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WithNullUserId_ShouldReturnFalse()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        bool result = await service.IsProfileCompleteAsync(null!, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WithEmptyUserId_ShouldReturnFalse()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        bool result = await service.IsProfileCompleteAsync("   ", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenRepositoryReturnsTrue_ShouldReturnTrue()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.IsProfileCompleteAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        bool result = await service.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_WhenRepositoryReturnsFalse_ShouldReturnFalse()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.IsProfileCompleteAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        bool result = await service.IsProfileCompleteAsync("user-1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProfileCompleteAsync_ShouldTrimUserId()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.IsProfileCompleteAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        _ = await service.IsProfileCompleteAsync("  user-1  ", CancellationToken.None);

        // Assert
        repositoryMock.Verify(r => r.IsProfileCompleteAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsProfileCompleteAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.IsProfileCompleteAsync("user-1", token))
            .ReturnsAsync(true);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        _ = await service.IsProfileCompleteAsync("user-1", token);

        // Assert
        repositoryMock.Verify(r => r.IsProfileCompleteAsync("user-1", token), Times.Once);
    }

    [Theory]
    [InlineData(null, "Doe", true, true, true, CommunicationPreference.Email, LanguagePreference.English, false)]
    [InlineData("", "Doe", true, true, true, CommunicationPreference.Email, LanguagePreference.English, false)]
    [InlineData("John", null, true, true, true, CommunicationPreference.Email, LanguagePreference.English, false)]
    [InlineData("John", "Doe", false, true, true, CommunicationPreference.Email, LanguagePreference.English, false)]
    [InlineData("John", "Doe", true, false, true, CommunicationPreference.Email, LanguagePreference.English, false)]
    [InlineData("John", "Doe", true, true, false, CommunicationPreference.Email, LanguagePreference.English, false)]
    [InlineData("John", "Doe", true, true, true, CommunicationPreference.None, LanguagePreference.English, false)]
    [InlineData("John", "Doe", true, true, true, CommunicationPreference.Email, LanguagePreference.None, false)]
    [InlineData("John", "Doe", true, true, true, CommunicationPreference.Email, LanguagePreference.English, true)]
    public async Task GetProfileAsync_IsComplete_ShouldReflectProfileCompleteness(
        string? firstName,
        string? lastName,
        bool hasAddress,
        bool hasDateOfBirth,
        bool hasStreet,
        CommunicationPreference communicationPreference,
        LanguagePreference languagePreference,
        bool expectedIsComplete)
    {
        // Arrange
        Address? address = hasAddress
            ? new Address(hasStreet ? "123 Main St" : "", "Montreal", "Quebec", "H1A 1A1")
            : null;

        CitizenProfile profile = new(
            Guid.NewGuid(),
            "user-1",
            firstName ?? string.Empty,
            lastName ?? string.Empty,
            address,
            hasDateOfBirth ? new DateOnly(1990, 5, 15) : null,
            communicationPreference,
            languagePreference);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Result<CitizenProfileDto> result = await service.GetProfileAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsComplete.Should().Be(expectedIsComplete);
    }

    // ── UpdateIdentityAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateIdentityAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Func<Task> act = () => service.UpdateIdentityAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateIdentityAsync_WithInvalidUserId_ShouldReturnFailure(string? userId)
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new(userId!, "514-555-0000", "test@example.com");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.invalid_user_id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateIdentityAsync_WithEmptyPhoneNumber_ShouldReturnFailure(string? phone)
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("user-1", phone!, "test@example.com");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.phone_number_required");
    }

    [Fact]
    public async Task UpdateIdentityAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("user-1", "514-555-0000", "not-an-email");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.email_invalid");
    }

    [Fact]
    public async Task UpdateIdentityAsync_WithEmptyEmail_ShouldTreatAsNull()
    {
        // Arrange
        CitizenProfile profile = CreateProfile();
        UserAccount user = new("user-1", "john", string.Empty, "514-555-0000", false, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.UpdateCitizenIdentityAsync(
                It.Is<UpdateCitizenIdentity>(i => i.Email == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("user-1", "514-555-0000", "   ");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            r => r.UpdateCitizenIdentityAsync(
                It.Is<UpdateCitizenIdentity>(i => i.Email == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateIdentityAsync_WhenRepositoryReturnsFailure_ShouldPropagateError()
    {
        // Arrange
        ResultError duplicateError = new("users.duplicate_email", "The email is already in use.");
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.UpdateCitizenIdentityAsync(It.IsAny<UpdateCitizenIdentity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserAccount>(duplicateError));

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("user-1", "514-555-0000", "taken@example.com");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.duplicate_email");
    }

    [Fact]
    public async Task UpdateIdentityAsync_WhenProfileNotFound_ShouldReturnFailure()
    {
        // Arrange
        UserAccount user = new("user-1", "john", "john@example.com", "514-555-0000", true, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CitizenProfile?)null);

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.UpdateCitizenIdentityAsync(It.IsAny<UpdateCitizenIdentity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("user-1", "514-555-0000", "john@example.com");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.not_found");
    }

    [Fact]
    public async Task UpdateIdentityAsync_WhenSuccessful_ShouldReturnDtoWithIdentityFields()
    {
        // Arrange
        CitizenProfile profile = CreateProfile();
        UserAccount user = new("user-1", "john", "john@example.com", "514-555-0000", true, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.UpdateCitizenIdentityAsync(It.IsAny<UpdateCitizenIdentity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("user-1", "514-555-0000", "john@example.com");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("john@example.com");
        result.Value.PhoneNumber.Should().Be("514-555-0000");
        result.Value.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateIdentityAsync_ShouldTrimValues()
    {
        // Arrange
        CitizenProfile profile = CreateProfile();
        UserAccount user = new("user-1", "john", "john@example.com", "514-555-0000", false, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.UpdateCitizenIdentityAsync(
                It.Is<UpdateCitizenIdentity>(i =>
                    i.UserId == "user-1" &&
                    i.PhoneNumber == "514-555-0000" &&
                    i.Email == "john@example.com"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("  user-1  ", "  514-555-0000  ", "  john@example.com  ");

        // Act
        Result<CitizenProfileDto> result = await service.UpdateIdentityAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userRepositoryMock.Verify(
            r => r.UpdateCitizenIdentityAsync(
                It.Is<UpdateCitizenIdentity>(i =>
                    i.UserId == "user-1" &&
                    i.PhoneNumber == "514-555-0000" &&
                    i.Email == "john@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateIdentityAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        UserAccount user = new("user-1", "john", "john@example.com", "514-555-0000", false, false, UserRole.Citizen);
        CitizenProfile profile = CreateProfile();

        Mock<ICitizenProfileRepository> repositoryMock = new();
        repositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1", token))
            .ReturnsAsync(profile);

        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.UpdateCitizenIdentityAsync(It.IsAny<UpdateCitizenIdentity>(), token))
            .ReturnsAsync(Result.Success(user));

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        UpdateCitizenIdentityRequest request = new("user-1", "514-555-0000", "john@example.com");

        // Act
        _ = await service.UpdateIdentityAsync(request, token);

        // Assert
        userRepositoryMock.Verify(
            r => r.UpdateCitizenIdentityAsync(It.IsAny<UpdateCitizenIdentity>(), token),
            Times.Once);
        repositoryMock.Verify(r => r.GetByUserIdAsync("user-1", token), Times.Once);
    }

    // ── SendEmailConfirmationAsync ───────────────────────────────────────────

    [Fact]
    public async Task SendEmailConfirmationAsync_WithNullUri_ShouldThrowArgumentNullException()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        // Act
        Func<Task> act = () => service.SendEmailConfirmationAsync("user-1", null!, LanguagePreference.English, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailConfirmationAsync_WithInvalidUserId_ShouldReturnFailure(string? userId)
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        Result result = await service.SendEmailConfirmationAsync(userId!, baseUrl, LanguagePreference.English, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("citizen_profile.invalid_user_id");
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount?)null);

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        Result result = await service.SendEmailConfirmationAsync("user-1", baseUrl, LanguagePreference.English, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailConfirmationAsync_WhenUserHasNoEmail_ShouldReturnFailure(string? email)
    {
        // Arrange
        UserAccount user = new("user-1", "john", email!, null, false, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        Result result = await service.SendEmailConfirmationAsync("user-1", baseUrl, LanguagePreference.English, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.email_required");
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WhenEmailAlreadyConfirmed_ShouldReturnFailure()
    {
        // Arrange
        UserAccount user = new("user-1", "john", "john@example.com", null, true, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        Result result = await service.SendEmailConfirmationAsync("user-1", baseUrl, LanguagePreference.English, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.already_confirmed");
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WhenTokenGenerationFails_ShouldPropagateError()
    {
        // Arrange
        UserAccount user = new("user-1", "john", "john@example.com", null, false, false, UserRole.Citizen);
        ResultError tokenError = new("users.not_found", "The user could not be found.");

        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(r => r.GenerateEmailConfirmationTokenAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(tokenError));

        Mock<IEmailSender> emailSenderMock = new();
        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        Result result = await service.SendEmailConfirmationAsync("user-1", baseUrl, LanguagePreference.English, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WhenEmailSendFails_ShouldPropagateError()
    {
        // Arrange
        UserAccount user = new("user-1", "john", "john@example.com", null, false, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(r => r.GenerateEmailConfirmationTokenAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("token-abc"));

        ResultError sendError = new("email.send_failed", "The email could not be sent.");
        Mock<IEmailSender> emailSenderMock = new();
        emailSenderMock
            .Setup(s => s.SendAsync(
                "john@example.com",
                "john",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(sendError));

        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        Result result = await service.SendEmailConfirmationAsync("user-1", baseUrl, LanguagePreference.English, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("email.send_failed");
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WhenSuccessful_ShouldSendEmailWithCorrectLink()
    {
        // Arrange
        UserAccount user = new("user-1", "john", "john@example.com", null, false, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(r => r.GenerateEmailConfirmationTokenAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("token-abc"));

        string? capturedHtml = null;
        Mock<IEmailSender> emailSenderMock = new();
        emailSenderMock
            .Setup(s => s.SendAsync(
                "john@example.com",
                "john",
                "Confirm your email address",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, string, string, CancellationToken>((_, _, _, html, _) => capturedHtml = html)
            .ReturnsAsync(Result.Success());

        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        Result result = await service.SendEmailConfirmationAsync("user-1", baseUrl, LanguagePreference.English, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedHtml.Should().NotBeNull();
        capturedHtml.Should().Contain("userId=user-1");
        capturedHtml.Should().Contain("token=token-abc");
        capturedHtml.Should().Contain("https://example.com/confirm");
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        UserAccount user = new("user-1", "john", "john@example.com", null, false, false, UserRole.Citizen);

        Mock<ICitizenProfileRepository> repositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock
            .Setup(r => r.GetByIdAsync("user-1", token))
            .ReturnsAsync(user);
        userRepositoryMock
            .Setup(r => r.GenerateEmailConfirmationTokenAsync("user-1", token))
            .ReturnsAsync(Result.Success("token-abc"));

        Mock<IEmailSender> emailSenderMock = new();
        emailSenderMock
            .Setup(s => s.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                token))
            .ReturnsAsync(Result.Success());

        CitizenProfileService service = new(repositoryMock.Object, userRepositoryMock.Object, emailSenderMock.Object, NullLogger<CitizenProfileService>.Instance);

        Uri baseUrl = new("https://example.com/confirm");

        // Act
        _ = await service.SendEmailConfirmationAsync("user-1", baseUrl, LanguagePreference.English, token);

        // Assert
        userRepositoryMock.Verify(r => r.GetByIdAsync("user-1", token), Times.Once);
        userRepositoryMock.Verify(r => r.GenerateEmailConfirmationTokenAsync("user-1", token), Times.Once);
        emailSenderMock.Verify(
            s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), token),
            Times.Once);
    }
}
