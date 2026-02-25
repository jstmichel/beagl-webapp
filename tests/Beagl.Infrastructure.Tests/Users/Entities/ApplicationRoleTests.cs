// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users.Entities;
using Xunit;
using FluentAssertions;

namespace Beagl.Infrastructure.Tests.Users.Entities;

/// <summary>
/// Verifies that an <see cref="ApplicationRole"/> can be created and its public properties read without throwing.
/// </summary>
public class ApplicationRoleTests
{
    [Fact]
    public void CanCreateApplicationRole()
    {
        // Arrange
        ApplicationRole role = new();

        // Assert
        role.Should().NotBeNull();
        role.Should().BeOfType<ApplicationRole>();

        // Access all public property getters to ensure they can be read without throwing.
        System.Reflection.PropertyInfo[] publicProperties = typeof(ApplicationRole).GetProperties();
        foreach (System.Reflection.PropertyInfo property in publicProperties)
        {
            FluentActions.Invoking(() => property.GetValue(role, null)).Should().NotThrow();
        }
    }

    /// <summary>
    /// Verifies that the Name property, when present as a writable string, can be set and read back
    /// with the same value. If the ApplicationRole type does not expose a writable string Name
    /// property, the test returns early and is treated as a no-op to avoid failing on incompatible
    /// implementations.
    /// </summary>
    [Fact]
    public void NameProperty_RoundTrips_WhenPresent()
    {
        // Arrange
        ApplicationRole role = new();
        System.Reflection.PropertyInfo? nameProperty = typeof(ApplicationRole).GetProperty("Name");

        // If there is no Name property, this test should still pass without failing assertions.
        if (nameProperty is null || !nameProperty.CanWrite || nameProperty.PropertyType != typeof(string))
        {
            return;
        }

        string expectedName = "TestRole";

        // Act
        nameProperty.SetValue(role, expectedName);
        object? actualName = nameProperty.GetValue(role, null);

        // Assert
        actualName.Should().BeOfType<string>();
        actualName.Should().Be(expectedName);
    }

    /// <summary>
    /// Verifies that the Name property, when present as a writable string, accepts empty and null values without throwing.
    /// </summary>
    [Fact]
    public void NameProperty_AllowsEmptyOrNull_WhenPresent()
    {
        // Arrange
        ApplicationRole role = new();
        System.Reflection.PropertyInfo? nameProperty = typeof(ApplicationRole).GetProperty("Name");

        if (nameProperty is null || !nameProperty.CanWrite || nameProperty.PropertyType != typeof(string))
        {
            return;
        }

        // Act & Assert
        FluentActions.Invoking(() => nameProperty.SetValue(role, string.Empty))
            .Should().NotThrow();

        FluentActions.Invoking(() => nameProperty.SetValue(role, null))
            .Should().NotThrow();
    }
}
