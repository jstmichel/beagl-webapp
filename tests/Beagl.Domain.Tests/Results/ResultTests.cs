// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Domain.Results;
using FluentAssertions;

namespace Beagl.Domain.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        Result result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithError()
    {
        // Arrange
        ResultError error = new("users.invalid_id", "A user identifier is required.");

        // Act
        Result result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithNullError_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => Result.Failure(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenericSuccess_ShouldExposeValueWithoutError()
    {
        // Act
        Result<string> result = Result.Success("ok");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("ok");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_ShouldExposeErrorAndDefaultValue()
    {
        // Arrange
        ResultError error = new("users.not_found", "The requested user could not be found.");

        // Act
        Result<string> result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Value.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_WithNullError_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => Result.Failure<string>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenericFailure_ForValueType_ShouldUseDefaultValue()
    {
        // Arrange
        ResultError error = new("users.not_found", "The requested user could not be found.");

        // Act
        Result<int> result = Result.Failure<int>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Value.Should().Be(0);
    }

    [Fact]
    public void Constructor_WhenSuccessContainsError_ShouldThrowArgumentException()
    {
        // Arrange
        ResultError error = new("users.invalid", "Invalid result state.");

        // Act
        Action act = () => _ = new ResultProbe(true, error);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenFailureHasNoError_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new ResultProbe(false, null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class ResultProbe(bool isSuccess, ResultError? error) : Result(isSuccess, error);
}
