// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;

namespace Beagl.Infrastructure.Users.Entities;

/// <summary>
/// Represents an application role, extending ASP.NET Core IdentityRole.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplicationRole : Microsoft.AspNetCore.Identity.IdentityRole
{
}
