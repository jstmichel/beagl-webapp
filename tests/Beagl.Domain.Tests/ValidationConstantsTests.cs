// MIT License - Copyright (c) 2025 Jonathan St-Michel

using FluentAssertions;

namespace Beagl.Domain.Tests;

public sealed class ValidationConstantsTests
{
    [Fact]
    public void UserNameMaxLength_ShouldBe256()
    {
        ValidationConstants.UserNameMaxLength.Should().Be(256);
    }

    [Fact]
    public void PasswordMinLength_ShouldBe8()
    {
        ValidationConstants.PasswordMinLength.Should().Be(8);
    }
}
