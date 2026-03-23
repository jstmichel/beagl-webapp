// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Bunit;
using Beagl.Application.Users.Dtos;
using Beagl.Application.Users.Services;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.WebApp.Components.Pages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Beagl.WebApp.Tests.Components.Pages;

public sealed class UsersPageTests : TestContext
{
    [Fact]
    public void Render_WithNoUsers_ShouldShowEmptyState()
    {
        // Arrange
        FakeUserManagementService fakeService = new()
        {
            Users = [],
        };
        Services.AddSingleton<IUserManagementService>(fakeService);
        Services.AddLogging();

        // Act
        IRenderedComponent<Users> component = RenderComponent<Users>();

        // Assert
        component.Find("[data-testid='empty-state']").TextContent.Should().Contain("No users match the current view");
    }

    [Fact]
    public void ClickingCreateUser_ShouldOpenCreatePanel()
    {
        // Arrange
        FakeUserManagementService fakeService = new()
        {
            Users = [new UserListItemDto("user-1", "alex", "alex@example.com", null, true, false, UserRole.Employee)],
        };
        Services.AddSingleton<IUserManagementService>(fakeService);
        Services.AddLogging();

        IRenderedComponent<Users> component = RenderComponent<Users>();

        // Act
        component.Find("[data-testid='create-user-button']").Click();

        // Assert
        component.Markup.Should().Contain("Create a new user");
        component.Markup.Should().Contain("Temporary password");
    }

    [Fact]
    public void ClickingDetails_ShouldShowSelectedUserDetails()
    {
        // Arrange
        FakeUserManagementService fakeService = new()
        {
            Users = [new UserListItemDto("user-1", "alex", "alex@example.com", "555-0100", true, false, UserRole.Employee)],
            SelectedUser = new UserDetailsDto("user-1", "alex", "alex@example.com", "555-0100", true, false, UserRole.Employee),
        };
        Services.AddSingleton<IUserManagementService>(fakeService);
        Services.AddLogging();

        IRenderedComponent<Users> component = RenderComponent<Users>();

        // Act
        component.Find("[data-testid='details-user-1']").Click();

        // Assert
        component.WaitForAssertion(() =>
        {
            component.Find("[data-testid='details-panel']").TextContent.Should().Contain("alex@example.com");
        });
    }

    private sealed class FakeUserManagementService : IUserManagementService
    {
        public IReadOnlyList<UserListItemDto> Users { get; init; } = [];

        public UserDetailsDto? SelectedUser { get; init; }

        public Task<UsersMetricsDto> GetUsersMetricsAsync(CancellationToken cancellationToken)
        {
            UsersMetricsDto metrics = new(Users.Count, Users.Count(user => !user.EmailConfirmed), Users.Count(user => user.IsLockedOut));
            return Task.FromResult(metrics);
        }

        public Task<Result<UserDetailsDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
        {
            UserDetailsDto createdUser = new(
                "created-user",
                request.UserName,
                request.Email ?? string.Empty,
                request.PhoneNumber,
                false,
                false,
                request.Role);

            return Task.FromResult(Result.Success(createdUser));
        }

        public Task<Result> DeleteAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<Result<UserDetailsDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
        {
            UserDetailsDto user = SelectedUser ?? new UserDetailsDto(userId, "alex", "alex@example.com", null, false, false, UserRole.Employee);
            return Task.FromResult(Result.Success(user));
        }

        public Task<UsersPageDto> GetUsersPageAsync(GetUsersPageRequest request, CancellationToken cancellationToken)
        {
            UsersPageDto page = new(Users, Users.Count, request.PageNumber, request.PageSize);
            return Task.FromResult(page);
        }

        public Task<Result<UserDetailsDto>> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            UserDetailsDto updatedUser = new(
                request.Id,
                request.UserName,
                request.Email ?? string.Empty,
                request.PhoneNumber,
                false,
                false,
                request.Role);

            return Task.FromResult(Result.Success(updatedUser));
        }
    }
}
