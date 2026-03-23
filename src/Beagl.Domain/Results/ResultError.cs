// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Results;

/// <summary>
/// Represents a domain or application error.
/// </summary>
/// <param name="Code">The machine-readable error code.</param>
/// <param name="Message">The human-readable error message.</param>
public sealed record ResultError(string Code, string Message);