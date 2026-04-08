// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Reflection;
using Beagl.Application.EmailProviders.Dtos;
using Beagl.Application.EmailProviders.Services;
using Beagl.Domain.Results;
using Beagl.WebApp.Components.Admin;
using Beagl.WebApp.Resources;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace Beagl.WebApp.Tests.Components.Admin;

public sealed class EmailConfigTabTests
{
    private readonly Mock<IEmailProviderConfigService> _serviceMock = new();
    private readonly Mock<IStringLocalizer<EmailConfigResource>> _localizerMock = new();

    private EmailConfigTab CreateComponent()
    {
        EmailConfigTab component = new();

        typeof(EmailConfigTab)
            .GetProperty("EmailProviderConfigService", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(component, _serviceMock.Object);

        typeof(EmailConfigTab)
            .GetProperty("L", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(component, _localizerMock.Object);

        return component;
    }

    private static T GetField<T>(EmailConfigTab component, string fieldName) where T : notnull
    {
        return (T)typeof(EmailConfigTab)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(component)!;
    }

    private static object? GetRawField(EmailConfigTab component, string fieldName)
    {
        return typeof(EmailConfigTab)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(component);
    }

    private static void SetField(EmailConfigTab component, string fieldName, object? value)
    {
        typeof(EmailConfigTab)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(component, value);
    }

    private static async Task InvokePrivateMethodAsync(EmailConfigTab component, string methodName)
    {
        Task task = (Task)typeof(EmailConfigTab)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, null)!;

        await task;
    }

    private static void InvokePrivateMethod(EmailConfigTab component, string methodName)
    {
        typeof(EmailConfigTab)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, null);
    }

    [Fact]
    public async Task LoadConfigAsync_WhenConfigExists_SetsConfigField()
    {
        EmailProviderConfigDto config = new(Guid.NewGuid(), "****1234", "test@example.com", "Test Name");

        _serviceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        EmailConfigTab component = CreateComponent();

        await InvokePrivateMethodAsync(component, "LoadConfigAsync");

        EmailProviderConfigDto? loaded = GetRawField(component, "_config") as EmailProviderConfigDto;
        loaded.Should().Be(config);
    }

    [Fact]
    public async Task LoadConfigAsync_WhenNoConfigExists_LeavesConfigNull()
    {
        _serviceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailProviderConfigDto?)null);

        EmailConfigTab component = CreateComponent();

        await InvokePrivateMethodAsync(component, "LoadConfigAsync");

        EmailProviderConfigDto? loaded = GetRawField(component, "_config") as EmailProviderConfigDto;
        loaded.Should().BeNull();
    }

    [Fact]
    public void OpenCreateForm_ResetsFormAndShowsIt()
    {
        EmailConfigTab component = CreateComponent();

        InvokePrivateMethod(component, "OpenCreateForm");

        bool showForm = GetField<bool>(component, "_showForm");
        showForm.Should().BeTrue();
    }

    [Fact]
    public void OpenEditForm_WhenConfigIsSet_PopulatesFormAndShowsIt()
    {
        EmailProviderConfigDto config = new(Guid.NewGuid(), "****5678", "edit@example.com", "Edit Name");
        EmailConfigTab component = CreateComponent();
        SetField(component, "_config", config);

        InvokePrivateMethod(component, "OpenEditForm");

        bool showForm = GetField<bool>(component, "_showForm");
        showForm.Should().BeTrue();

        object form = GetField<object>(component, "_form");
        string senderEmail = (string)form.GetType().GetProperty("SenderEmail")!.GetValue(form)!;
        string senderName = (string)form.GetType().GetProperty("SenderName")!.GetValue(form)!;
        senderEmail.Should().Be("edit@example.com");
        senderName.Should().Be("Edit Name");
    }

    [Fact]
    public void CancelEdit_HidesFormAndClearsErrors()
    {
        EmailConfigTab component = CreateComponent();
        SetField(component, "_showForm", true);

        List<string> formErrors = GetField<List<string>>(component, "_formErrors");
        formErrors.Add("some error");

        InvokePrivateMethod(component, "CancelEdit");

        bool showForm = GetField<bool>(component, "_showForm");
        showForm.Should().BeFalse();
        formErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_OnSuccess_UpdatesConfigAndHidesForm()
    {
        EmailProviderConfigDto savedConfig = new(Guid.NewGuid(), "****9999", "saved@example.com", "Saved");

        _serviceMock
            .Setup(s => s.SaveAsync(It.IsAny<SaveEmailProviderConfigRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(savedConfig));

        EmailConfigTab component = CreateComponent();
        SetField(component, "_showForm", true);

        await InvokePrivateMethodAsync(component, "SaveAsync");

        EmailProviderConfigDto? config = GetRawField(component, "_config") as EmailProviderConfigDto;
        bool showForm = GetField<bool>(component, "_showForm");
        config.Should().Be(savedConfig);
        showForm.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_OnFailure_AddsLocalizedErrorToFormErrors()
    {
        ResultError error = new("email.save_error", "Save failed");
        LocalizedString localizedError = new("email.save_error", "Échec de la sauvegarde");

        _serviceMock
            .Setup(s => s.SaveAsync(It.IsAny<SaveEmailProviderConfigRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<EmailProviderConfigDto>(error));

        _localizerMock.As<IStringLocalizer>()
            .Setup(l => l["email.save_error"])
            .Returns(localizedError);

        EmailConfigTab component = CreateComponent();

        await InvokePrivateMethodAsync(component, "SaveAsync");

        List<string> formErrors = GetField<List<string>>(component, "_formErrors");
        formErrors.Should().ContainSingle().Which.Should().Be("Échec de la sauvegarde");
    }

    [Fact]
    public void Dispose_CancelsTokenSource()
    {
        EmailConfigTab component = CreateComponent();
        CancellationTokenSource cts = GetField<CancellationTokenSource>(component, "_cts");

        component.Dispose();

        cts.IsCancellationRequested.Should().BeTrue();
    }
}
