// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Dtos;
using Beagl.Domain.Users;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Users;

/// <summary>
/// Component that renders user detail sections.
/// </summary>
public sealed partial class UserDetailsView
{
    /// <summary>Gets or sets the user to display.</summary>
    [Parameter, EditorRequired]
    public UserDetailsDto User { get; set; } = default!;

    /// <summary>Gets or sets a value indicating whether a save operation is in progress.</summary>
    [Parameter]
    public bool IsSaving { get; set; }

    /// <summary>Gets or sets the callback invoked when the edit action is triggered.</summary>
    [Parameter]
    public EventCallback OnEdit { get; set; }

    /// <summary>Gets or sets the callback invoked when the delete action is triggered.</summary>
    [Parameter]
    public EventCallback OnDelete { get; set; }

    /// <summary>Gets or sets the callback invoked when the copy confirmation link action is triggered.</summary>
    [Parameter]
    public EventCallback OnCopyConfirmationLink { get; set; }

    /// <summary>Gets or sets the callback invoked when the panel is closed.</summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    [Inject]
    private IStringLocalizer<UsersResource> L { get; set; } = default!;

    private string FormatOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? L["Users.Optional.NotProvided"] : value;
    }

    private string LocalizeRole(UserRole role)
    {
        LocalizedString localizedRole = L[$"Users.Role.{role}"];
        return localizedRole.ResourceNotFound ? role.ToString() : localizedRole.Value;
    }
}
