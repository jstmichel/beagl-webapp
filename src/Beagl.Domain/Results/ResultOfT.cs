// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Results;

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
/// <typeparam name="TValue">The type of the returned value.</typeparam>
public sealed class Result<TValue> : Result
{
    internal Result(TValue? value, bool isSuccess, ResultError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value associated with a successful result.
    /// </summary>
    public TValue? Value { get; }

}