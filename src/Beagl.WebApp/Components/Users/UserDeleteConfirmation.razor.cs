// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.Users.Dtos;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Users;

/// <summary>
/// Component that renders a user deletion confirmation prompt.
/// </summary>
public sealed partial class UserDeleteConfirmation
{
    /// <summary>Gets or sets the user to delete.</summary>
    [Parameter, EditorRequired]
    public UserDetailsDto User { get; set; } = default!;

    /// <summary>Gets or sets a value indicating whether a save operation is in progress.</summary>
    [Parameter]
    public bool IsSaving { get; set; }

    /// <summary>Gets or sets the callback invoked when deletion is confirmed.</summary>
    [Parameter]
    public EventCallback OnConfirm { get; set; }

    /// <summary>Gets or sets the callback invoked when deletion is cancelled.</summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Inject]
    private IStringLocalizer<UsersResource> L { get; set; } = default!;
}
