// MIT License - Copyright (c) 2025 Jonathan St-Michel

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
    public void Properties_WhenAssigned_ShouldRetainValues()
    {
        Guid id = Guid.NewGuid();
        CitizenProfileEntity entity = new()
        {
            Id = id,
            UserId = "user-123",
            FirstName = "Jane",
            LastName = "Doe",
        };

        entity.Id.Should().Be(id);
        entity.UserId.Should().Be("user-123");
        entity.FirstName.Should().Be("Jane");
        entity.LastName.Should().Be("Doe");
    }
}
