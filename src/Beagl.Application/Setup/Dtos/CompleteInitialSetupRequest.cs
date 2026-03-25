// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Setup.Dtos;

/// <summary>
/// Represents the request payload used to complete first-run setup.
/// </summary>
/// <param name="UserName">The administrator user name.</param>
/// <param name="Email">The administrator email address.</param>
/// <param name="Password">The administrator password.</param>
public sealed record CompleteInitialSetupRequest(
    string UserName,
    string Email,
    string Password);
