// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Users;
using Beagl.Infrastructure.Database;
using Beagl.Infrastructure.Users.Entities;
using FluentAssertions;

namespace Beagl.Infrastructure.Tests.Users;

public sealed class CitizenProfileEntityTests
{
    [Fact]
    public void DefaultId_ShouldBeEmpty()
    {
        CitizenProfileEntity entity = new();

        entity.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void DefaultUserId_ShouldBeEmptyString()
    {
        CitizenProfileEntity entity = new();

        entity.UserId.Should().Be(string.Empty);
    }

    [Fact]
    public void DefaultFirstName_ShouldBeEmptyString()
    {
        CitizenProfileEntity entity = new();

        entity.FirstName.Should().Be(string.Empty);
    }

    [Fact]
    public void DefaultLastName_ShouldBeEmptyString()
    {
        CitizenProfileEntity entity = new();

        entity.LastName.Should().Be(string.Empty);
    }

    [Fact]
    public void DefaultAddress_ShouldBeNull()
    {
        CitizenProfileEntity entity = new();

        entity.Address.Should().BeNull();
    }

    [Fact]
    public void DefaultDateOfBirth_ShouldBeNull()
    {
        CitizenProfileEntity entity = new();

        entity.DateOfBirth.Should().BeNull();
    }

    [Fact]
    public void DefaultCommunicationPreference_ShouldBeNone()
    {
        CitizenProfileEntity entity = new();

        entity.CommunicationPreference.Should().Be(CommunicationPreference.None);
    }

    [Fact]
    public void DefaultLanguagePreference_ShouldBeNone()
    {
        CitizenProfileEntity entity = new();

        entity.LanguagePreference.Should().Be(LanguagePreference.None);
    }

    [Fact]
    public void Properties_WhenAssigned_ShouldRetainValues()
    {
        Guid id = Guid.NewGuid();
        AddressEntity address = new()
        {
            Street = "456 Oak Ave",
            City = "Toronto",
            Province = "Ontario",
            PostalCode = "M5A 1A1",
        };

        CitizenProfileEntity entity = new()
        {
            Id = id,
            UserId = "user-123",
            FirstName = "Jane",
            LastName = "Doe",
            Address = address,
            DateOfBirth = new DateOnly(1985, 3, 20),
            CommunicationPreference = CommunicationPreference.Phone,
            LanguagePreference = LanguagePreference.French,
        };

        entity.Id.Should().Be(id);
        entity.UserId.Should().Be("user-123");
        entity.FirstName.Should().Be("Jane");
        entity.LastName.Should().Be("Doe");
        entity.Address.Should().NotBeNull();
        entity.Address!.Street.Should().Be("456 Oak Ave");
        entity.Address.City.Should().Be("Toronto");
        entity.Address.Province.Should().Be("Ontario");
        entity.Address.PostalCode.Should().Be("M5A 1A1");
        entity.DateOfBirth.Should().Be(new DateOnly(1985, 3, 20));
        entity.CommunicationPreference.Should().Be(CommunicationPreference.Phone);
        entity.LanguagePreference.Should().Be(LanguagePreference.French);
    }
}
