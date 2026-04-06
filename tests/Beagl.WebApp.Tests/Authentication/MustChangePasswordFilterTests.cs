// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Security.Claims;
using Beagl.Infrastructure.Users;
using Beagl.WebApp.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace Beagl.WebApp.Tests.Authentication;

public sealed class MustChangePasswordFilterTests
{
    [Fact]
    public async Task OnPageHandlerSelectionAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        MustChangePasswordFilter filter = new();
        DefaultHttpContext httpContext = new();
        CompiledPageActionDescriptor descriptor = new()
        {
            ViewEnginePath = "/Account/Profile",
            EndpointMetadata = [],
        };
        ActionContext actionContext = new(
            httpContext,
            new RouteData(),
            descriptor,
            new ModelStateDictionary());
        PageContext pageContext = new(actionContext);
        PageHandlerSelectedContext context = new(pageContext, [], new HandlerMethodDescriptor());

        // Act
        await filter.OnPageHandlerSelectionAsync(context);

        // Assert – no exception means pass
    }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenUserMustChangePassword_ShouldRedirect()
    {
        // Arrange
        MustChangePasswordFilter filter = new();
        PageHandlerExecutingContext context = CreateContext(
            pagePath: "/Account/Profile",
            hasMustChangePasswordClaim: true);
        bool nextCalled = false;

        // Act
        await filter.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<PageHandlerExecutedContext>(null!);
        });

        // Assert
        context.Result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/account/change-password");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenUserMustChangePassword_AndOnChangePasswordPage_ShouldContinue()
    {
        // Arrange
        MustChangePasswordFilter filter = new();
        PageHandlerExecutingContext context = CreateContext(
            pagePath: "/account/change-password",
            hasMustChangePasswordClaim: true);
        bool nextCalled = false;

        // Act
        await filter.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<PageHandlerExecutedContext>(null!);
        });

        // Assert
        context.Result.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenUserMustChangePassword_AndOnLogoutPage_ShouldContinue()
    {
        // Arrange
        MustChangePasswordFilter filter = new();
        PageHandlerExecutingContext context = CreateContext(
            pagePath: "/account/logout",
            hasMustChangePasswordClaim: true);
        bool nextCalled = false;

        // Act
        await filter.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<PageHandlerExecutedContext>(null!);
        });

        // Assert
        context.Result.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenUserDoesNotHaveClaim_ShouldContinue()
    {
        // Arrange
        MustChangePasswordFilter filter = new();
        PageHandlerExecutingContext context = CreateContext(
            pagePath: "/Account/Profile",
            hasMustChangePasswordClaim: false);
        bool nextCalled = false;

        // Act
        await filter.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<PageHandlerExecutedContext>(null!);
        });

        // Assert
        context.Result.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenUserIsAnonymous_ShouldContinue()
    {
        // Arrange
        MustChangePasswordFilter filter = new();
        PageHandlerExecutingContext context = CreateContext(
            pagePath: "/Account/Login",
            hasMustChangePasswordClaim: false,
            isAuthenticated: false);
        bool nextCalled = false;

        // Act
        await filter.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<PageHandlerExecutedContext>(null!);
        });

        // Assert
        context.Result.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnPageHandlerExecutionAsync_WhenViewEnginePathIsNull_ShouldContinue()
    {
        // Arrange
        MustChangePasswordFilter filter = new();
        PageHandlerExecutingContext context = CreateContext(
            pagePath: null!,
            hasMustChangePasswordClaim: true);
        bool nextCalled = false;

        // Act
        await filter.OnPageHandlerExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<PageHandlerExecutedContext>(null!);
        });

        // Assert
        context.Result.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    private static PageHandlerExecutingContext CreateContext(
        string pagePath,
        bool hasMustChangePasswordClaim,
        bool isAuthenticated = true)
    {
        DefaultHttpContext httpContext = new();

        if (isAuthenticated)
        {
            List<Claim> claims = [new Claim(ClaimTypes.NameIdentifier, "user-1")];
            if (hasMustChangePasswordClaim)
            {
                claims.Add(new Claim(
                    ApplicationUserClaimsPrincipalFactory.MustChangePasswordClaimType, "true"));
            }

            ClaimsIdentity identity = new(claims, "TestAuth");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        CompiledPageActionDescriptor descriptor = new()
        {
            ViewEnginePath = pagePath,
            EndpointMetadata = [],
        };

        ActionContext actionContext = new(
            httpContext,
            new RouteData(),
            descriptor,
            new ModelStateDictionary());

        PageContext pageContext = new(actionContext);

        return new PageHandlerExecutingContext(
            pageContext,
            [],
            new HandlerMethodDescriptor(),
            new Dictionary<string, object?>(),
            new object());
    }
}
