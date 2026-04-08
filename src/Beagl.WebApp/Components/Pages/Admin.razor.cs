// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Pages;

/// <summary>
/// Code-behind for the administration portal page.
/// </summary>
public sealed partial class Admin
{
    private const string _tabEmail = "email";

    private string _activeTab = _tabEmail;

    [Inject]
    private IStringLocalizer<AdminResource> L { get; set; } = default!;

    private void SetTab(string tab)
    {
        _activeTab = tab;
    }
}
