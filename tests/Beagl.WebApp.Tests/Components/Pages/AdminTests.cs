// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Reflection;
using FluentAssertions;
using AdminPage = Beagl.WebApp.Components.Pages.Admin;

namespace Beagl.WebApp.Tests.Components.Pages;

public sealed class AdminTests
{
    [Fact]
    public void Admin_DefaultActiveTab_IsEmail()
    {
        AdminPage admin = new();
        FieldInfo field = typeof(AdminPage).GetField("_activeTab", BindingFlags.NonPublic | BindingFlags.Instance)!;

        string activeTab = (string)field.GetValue(admin)!;

        activeTab.Should().Be("email");
    }

    [Fact]
    public void SetTab_ChangesActiveTab_ToGivenValue()
    {
        AdminPage admin = new();
        MethodInfo method = typeof(AdminPage).GetMethod("SetTab", BindingFlags.NonPublic | BindingFlags.Instance)!;
        FieldInfo field = typeof(AdminPage).GetField("_activeTab", BindingFlags.NonPublic | BindingFlags.Instance)!;

        method.Invoke(admin, ["reports"]);

        string activeTab = (string)field.GetValue(admin)!;
        activeTab.Should().Be("reports");
    }
}
