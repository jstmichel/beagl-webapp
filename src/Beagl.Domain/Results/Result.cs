// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the operation succeeded.</param>
    /// <param name="error">The error associated with the failure.</param>
    protected Result(bool isSuccess, ResultError? error)
    {
        if (isSuccess && error is not null)
        {
            throw new ArgumentException("A successful result cannot include an error.", nameof(error));
        }

        if (!isSuccess && error is null)
        {
            throw new ArgumentNullException(nameof(error), "A failed result must include an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the result when the operation fails.
    /// </summary>
    public ResultError? Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error associated with the failure.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(ResultError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result(false, error);
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the returned value.</typeparam>
    /// <param name="value">The returned value.</param>
    /// <returns>A successful result containing the provided value.</returns>
    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, null);
    }

    /// <summary>
    /// Creates a failed result with a value type.
    /// </summary>
    /// <typeparam name="TValue">The type of the returned value.</typeparam>
    /// <param name="error">The error associated with the failure.</param>
    /// <returns>A failed result.</returns>
    public static Result<TValue> Failure<TValue>(ResultError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<TValue>(default, false, error);
    }
}