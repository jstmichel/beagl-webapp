// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Infrastructure.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Beagl.WebApp.Authentication;

/// <summary>
/// Redirects authenticated users to the change-password page when the MustChangePassword claim is set.
/// </summary>
internal sealed class MustChangePasswordFilter : IAsyncPageFilter
{
    private static readonly HashSet<string> _allowedPaths =
    [
        "/account/change-password",
        "/account/logout",
        "/account/access-denied",
    ];

    /// <inheritdoc />
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.ActionDescriptor is CompiledPageActionDescriptor descriptor
            && context.HttpContext.User.Identity is { IsAuthenticated: true }
            && context.HttpContext.User.HasClaim(
                ApplicationUserClaimsPrincipalFactory.MustChangePasswordClaimType, "true"))
        {
            string? pagePath = descriptor.ViewEnginePath;

            if (pagePath is not null && !_allowedPaths.Contains(pagePath, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectResult("/account/change-password");
                return;
            }
        }

        await next().ConfigureAwait(false);
    }
}
