// MIT License - Copyright (c) 2025 Jonathan St-Michel

using FluentAssertions;

namespace Beagl.Domain.Tests;

public sealed class EmailValidatorTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("admin@beagl.local", true)]
    [InlineData("not-an-email", false)]
    [InlineData("", false)]
    [InlineData(null, true)]
    public void IsValid_ShouldReturnExpectedResult(string? email, bool expected)
    {
        bool result = EmailValidator.IsValid(email);

        result.Should().Be(expected);
    }
}
