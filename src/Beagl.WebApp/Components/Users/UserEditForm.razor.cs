// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using Beagl.Application.Users.Dtos;
using Beagl.Domain;
using Beagl.Domain.Users;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Users;

/// <summary>
/// Form component for editing an existing user.
/// </summary>
public sealed partial class UserEditForm
{
    private readonly EditUserFormModel _form = new();
    private EditContext _editContext = default!;

    /// <summary>Gets or sets the user to edit.</summary>
    [Parameter, EditorRequired]
    public UserDetailsDto User { get; set; } = default!;

    /// <summary>Gets or sets a value indicating whether a save operation is in progress.</summary>
    [Parameter]
    public bool IsSaving { get; set; }

    /// <summary>Gets or sets the callback invoked when the form is submitted.</summary>
    [Parameter]
    public EventCallback<UpdateUserRequest> OnSubmit { get; set; }

    /// <summary>Gets or sets the callback invoked when the form is cancelled.</summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Inject]
    private IStringLocalizer<UsersResource> L { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _form.Load(User);
        _editContext = new EditContext(_form);
    }

    private async Task SubmitAsync()
    {
        UpdateUserRequest request = new(
            _form.Id,
            _form.UserName,
            _form.Email,
            _form.PhoneNumber,
            _form.Role);

        await OnSubmit.InvokeAsync(request);
    }

    private static IReadOnlyList<UserRole> SelectableRoles =>
    [
        UserRole.Citizen,
        UserRole.Employee,
        UserRole.Administrator,
    ];

    private string LocalizeRole(UserRole role)
    {
        LocalizedString localizedRole = L[$"Users.Role.{role}"];
        return localizedRole.ResourceNotFound ? role.ToString() : localizedRole.Value;
    }

    private sealed class EditUserFormModel : IValidatableObject
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public UserRole Role { get; set; } = UserRole.Employee;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                yield return new ValidationResult("users.user_name_required", [nameof(UserName)]);
            }
            else if (UserName.Trim().Length > ValidationConstants.UserNameMaxLength)
            {
                yield return new ValidationResult("users.user_name_too_long", [nameof(UserName)]);
            }

            if (Role is UserRole.Employee or UserRole.Administrator && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("users.email_required", [nameof(Email)]);
            }
            else if (!string.IsNullOrWhiteSpace(Email) && !EmailValidator.IsValid(Email.Trim()))
            {
                yield return new ValidationResult("users.invalid_email", [nameof(Email)]);
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult("users.phone_required", [nameof(PhoneNumber)]);
            }
        }

        public void Load(UserDetailsDto user)
        {
            Id = user.Id;
            UserName = user.UserName;
            Email = string.IsNullOrEmpty(user.Email) ? null : user.Email;
            PhoneNumber = user.PhoneNumber;
            Role = user.Role;
        }
    }
}
