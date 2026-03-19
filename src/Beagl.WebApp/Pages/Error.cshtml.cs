// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Beagl.WebApp.Pages;

/// <summary>
/// Represents the application error page.
/// </summary>
public sealed class ErrorModel : PageModel
{
    /// <summary>
    /// Gets the current request identifier.
    /// </summary>
    public string? RequestId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the request identifier is available.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>
    /// Populates page data on GET requests.
    /// </summary>
    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}
