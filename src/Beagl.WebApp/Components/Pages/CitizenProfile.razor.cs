// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Beagl.Application.CitizenProfiles.Dtos;
using Beagl.Application.CitizenProfiles.Services;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Beagl.WebApp.Extensions;
using Beagl.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace Beagl.WebApp.Components.Pages;

/// <summary>
/// Code-behind for the citizen profile page.
/// </summary>
public sealed partial class CitizenProfile : ComponentBase, IDisposable
{
    [Inject]
    private ICitizenProfileService CitizenProfileService { get; set; } = null!;

    [Inject]
    private IStringLocalizer<CitizenProfileResource> L { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private ProfileFormModel _model = new();
    private EditContext _editContext = null!;
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _isFirstLogin;
    private string? _successMessage;
    private string? _errorMessage;
    private string _userId = string.Empty;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        ArgumentNullException.ThrowIfNull(AuthenticationStateTask);

        AuthenticationState state = await AuthenticationStateTask.ConfigureAwait(false);
        _userId = state.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_userId))
        {
            NavigationManager.NavigateTo("/account/login", forceLoad: true);
            return;
        }

        _editContext = new EditContext(_model);

        Result<CitizenProfileDto> result = await CitizenProfileService
            .GetProfileAsync(_userId, CancellationToken.None)
            .ConfigureAwait(false);

        if (result.IsSuccess && result.Value is not null)
        {
            CitizenProfileDto profile = result.Value;
            _model.FirstName = profile.FirstName;
            _model.LastName = profile.LastName;
            _model.Street = profile.Address?.Street ?? string.Empty;
            _model.City = profile.Address?.City ?? string.Empty;
            _model.Province = profile.Address?.Province ?? string.Empty;
            _model.PostalCode = profile.Address?.PostalCode ?? string.Empty;
            _model.DateOfBirth = profile.DateOfBirth;
            _model.CommunicationPreference = profile.CommunicationPreference;
            _model.LanguagePreference = profile.LanguagePreference;
            _isFirstLogin = !profile.IsComplete;
        }
        else
        {
            _isFirstLogin = true;
        }

        _editContext = new EditContext(_model);
        _isLoading = false;
    }

    private async Task HandleSubmitAsync()
    {
        _successMessage = null;
        _errorMessage = null;
        _isSaving = true;

        UpdateCitizenProfileRequest request = new(
            _userId,
            _model.FirstName,
            _model.LastName,
            new Address(
                _model.Street,
                _model.City,
                _model.Province,
                _model.PostalCode),
            _model.DateOfBirth ?? default,
            _model.CommunicationPreference,
            _model.LanguagePreference);

        Result<CitizenProfileDto> result = await CitizenProfileService
            .UpdateProfileAsync(request, CancellationToken.None)
            .ConfigureAwait(false);

        _isSaving = false;

        if (result.IsSuccess)
        {
            _successMessage = L["CitizenProfile.Form.Success"];
            _isFirstLogin = false;
        }
        else
        {
            _errorMessage = L.LocalizeError(result.Error!);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // No resources to dispose currently.
    }

    /// <summary>
    /// Form model for the citizen profile page.
    /// </summary>
    internal sealed class ProfileFormModel : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street line.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the province.
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date of birth.
        /// </summary>
        public DateOnly? DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the communication preference.
        /// </summary>
        public CommunicationPreference CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets the language preference.
        /// </summary>
        public LanguagePreference LanguagePreference { get; set; }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                yield return new ValidationResult("citizen_profile.first_name_required", [nameof(FirstName)]);
            }
            else if (FirstName.Trim().Length > ValidationConstants.FirstNameMaxLength)
            {
                yield return new ValidationResult("citizen_profile.first_name_too_long", [nameof(FirstName)]);
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                yield return new ValidationResult("citizen_profile.last_name_required", [nameof(LastName)]);
            }
            else if (LastName.Trim().Length > ValidationConstants.LastNameMaxLength)
            {
                yield return new ValidationResult("citizen_profile.last_name_too_long", [nameof(LastName)]);
            }

            if (string.IsNullOrWhiteSpace(Street))
            {
                yield return new ValidationResult("citizen_profile.street_required", [nameof(Street)]);
            }
            else if (Street.Trim().Length > ValidationConstants.StreetMaxLength)
            {
                yield return new ValidationResult("citizen_profile.street_too_long", [nameof(Street)]);
            }

            if (string.IsNullOrWhiteSpace(City))
            {
                yield return new ValidationResult("citizen_profile.city_required", [nameof(City)]);
            }
            else if (City.Trim().Length > ValidationConstants.CityMaxLength)
            {
                yield return new ValidationResult("citizen_profile.city_too_long", [nameof(City)]);
            }

            if (string.IsNullOrWhiteSpace(Province))
            {
                yield return new ValidationResult("citizen_profile.province_required", [nameof(Province)]);
            }
            else if (Province.Trim().Length > ValidationConstants.ProvinceMaxLength)
            {
                yield return new ValidationResult("citizen_profile.province_too_long", [nameof(Province)]);
            }

            if (string.IsNullOrWhiteSpace(PostalCode))
            {
                yield return new ValidationResult("citizen_profile.postal_code_required", [nameof(PostalCode)]);
            }
            else if (PostalCode.Trim().Length > ValidationConstants.PostalCodeMaxLength)
            {
                yield return new ValidationResult("citizen_profile.postal_code_too_long", [nameof(PostalCode)]);
            }

            if (DateOfBirth is null)
            {
                yield return new ValidationResult("citizen_profile.date_of_birth_required", [nameof(DateOfBirth)]);
            }

            if (CommunicationPreference == CommunicationPreference.None
                || !Enum.IsDefined(CommunicationPreference))
            {
                yield return new ValidationResult(
                    "citizen_profile.communication_preference_required",
                    [nameof(CommunicationPreference)]);
            }

            if (LanguagePreference == LanguagePreference.None
                || !Enum.IsDefined(LanguagePreference))
            {
                yield return new ValidationResult(
                    "citizen_profile.language_preference_required",
                    [nameof(LanguagePreference)]);
            }
        }
    }
}
