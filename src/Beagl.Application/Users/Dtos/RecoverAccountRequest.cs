// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Users.Dtos;

/// <summary>
/// Represents the application request to recover an account using a recovery code.
/// </summary>
/// <param name="Code">The recovery code.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record RecoverAccountRequest(
    string Code,
    string NewPassword);
