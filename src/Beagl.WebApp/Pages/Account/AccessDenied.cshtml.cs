// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Beagl.WebApp.Pages.Account;

/// <summary>
/// Renders the access denied page for authenticated users without the required permissions.
/// </summary>
[AllowAnonymous]
internal sealed class AccessDeniedModel : PageModel
{
}
