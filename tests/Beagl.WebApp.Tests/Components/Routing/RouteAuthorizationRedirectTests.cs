// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using Beagl.WebApp.Components.Routing;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Beagl.WebApp.Tests.Components.Routing;

public sealed class RouteAuthorizationRedirectTests : TestContext
{
    [Fact]
    public void Render_ShouldRedirectAnonymousUsersToLoginWithReturnUrl()
    {
        Services.AddAuthorizationCore();
        Services.AddCascadingAuthenticationState();

        TestAuthorizationContext authorizationContext = this.AddTestAuthorization();
        authorizationContext.SetNotAuthorized();

        NavigationManager navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo("/employee/users");

        RenderComponent<CascadingAuthenticationState>(parameters => parameters
            .AddChildContent<RouteAuthorizationRedirect>());

        navigationManager.Uri.Should().Be("http://localhost/account/login?returnUrl=%2Femployee%2Fusers");
    }

    [Fact]
    public void Render_ShouldRedirectAuthenticatedUsersToAccessDenied()
    {
        Services.AddAuthorizationCore();
        Services.AddCascadingAuthenticationState();

        TestAuthorizationContext authorizationContext = this.AddTestAuthorization();
        authorizationContext.SetAuthorized("employee1");
        authorizationContext.SetClaims(new Claim(ClaimTypes.Role, "Employee"));

        NavigationManager navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo("/employee/users");

        RenderComponent<CascadingAuthenticationState>(parameters => parameters
            .AddChildContent<RouteAuthorizationRedirect>());

        navigationManager.Uri.Should().Be("http://localhost/account/access-denied");
    }
}
