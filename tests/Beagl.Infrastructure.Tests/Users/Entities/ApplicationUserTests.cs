// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users.Entities;
using Xunit;
using FluentAssertions;

namespace Beagl.Infrastructure.Tests.Users.Entities;

/// <summary>
/// Tests for the <see cref="ApplicationUser"/> infrastructure entity.
/// </summary>
public class ApplicationUserTests
{
    /// <summary>
    /// Ensures that an <see cref="ApplicationUser"/> can be instantiated.
    /// This is a basic construction smoke test.
    /// </summary>
    [Fact]
    public void CanCreateApplicationUser()
    {
        // Act
        ApplicationUser user = new();

        // Assert
        user.Should().NotBeNull();
        user.Should().BeOfType<ApplicationUser>();
    }

    /// <summary>
    /// Happy path: setting common properties on <see cref="ApplicationUser"/> should persist their values.
    /// </summary>
    [Fact]
    public void SettingUserName_ShouldPersistValue()
    {
        // Arrange
        ApplicationUser user = new();
        string expectedUserName = "happy-path-user";

        // Act
        user.UserName = expectedUserName;

        // Assert
        user.UserName.Should().Be(expectedUserName);
    }

    /// <summary>
    /// Edge case: <see cref="ApplicationUser.UserName"/> should accept an empty string and persist it.
    /// </summary>
    [Fact]
    public void SettingEmptyUserName_ShouldBeAllowed()
    {
        // Arrange
        ApplicationUser user = new();
        string emptyUserName = string.Empty;

        // Act
        user.UserName = emptyUserName;

        // Assert
        user.UserName.Should().Be(emptyUserName);
    }

    /// <summary>
    /// Invariant: separate <see cref="ApplicationUser"/> instances should have distinct identifiers.
    /// This guards against accidental identifier reuse.
    /// </summary>
    [Fact]
    public void DifferentUsers_ShouldHaveDifferentIds()
    {
        // Act
        ApplicationUser firstUser = new();
        ApplicationUser secondUser = new();

        // Assert
        firstUser.Id.Should().NotBeNullOrEmpty();
        secondUser.Id.Should().NotBeNullOrEmpty();
        firstUser.Id.Should().NotBe(secondUser.Id);
    }
}
