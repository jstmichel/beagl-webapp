// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;
using Beagl.Domain.Users;

namespace Beagl.WebApp.Authentication;

/// <summary>
/// Defines authorization constants used by the employee workspace.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class AuthorizationPolicies
{
    /// <summary>
    /// Gets the policy name for employee workspace access.
    /// </summary>
    public const string EmployeeAccess = nameof(EmployeeAccess);

    /// <summary>
    /// Gets the employee role name.
    /// </summary>
    public const string EmployeeRole = nameof(UserRole.Employee);

    /// <summary>
    /// Gets the administrator role name.
    /// </summary>
    public const string AdministratorRole = nameof(UserRole.Administrator);
}
