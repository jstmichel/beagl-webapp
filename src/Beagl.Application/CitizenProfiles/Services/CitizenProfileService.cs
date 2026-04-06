// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Beagl.Application.CitizenProfiles.Dtos;
using Beagl.Application.EmailProviders.Services;
using Beagl.Domain;
using Beagl.Domain.Results;
using Beagl.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Beagl.Application.CitizenProfiles.Services;

/// <summary>
/// Implements citizen profile management workflows.
/// </summary>
/// <param name="citizenProfileRepository">The citizen profile repository.</param>
/// <param name="userRepository">The user repository.</param>
/// <param name="emailSender">The email sender.</param>
/// <param name="emailTemplateService">The email template service.</param>
/// <param name="logger">The logger.</param>
public sealed partial class CitizenProfileService(
    ICitizenProfileRepository citizenProfileRepository,
    IUserRepository userRepository,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ILogger<CitizenProfileService> logger) : ICitizenProfileService
{
    private readonly ICitizenProfileRepository _citizenProfileRepository =
        citizenProfileRepository ?? throw new ArgumentNullException(nameof(citizenProfileRepository));

    private readonly IUserRepository _userRepository =
        userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    private readonly IEmailSender _emailSender =
        emailSender ?? throw new ArgumentNullException(nameof(emailSender));

    private readonly IEmailTemplateService _emailTemplateService =
        emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));

    private readonly ILogger<CitizenProfileService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<Result<CitizenProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure<CitizenProfileDto>(
                new ResultError("citizen_profile.invalid_user_id", "A user identifier is required."));
        }

        string trimmedUserId = userId.Trim();

        CitizenProfile? profile = await _citizenProfileRepository
            .GetByUserIdAsync(trimmedUserId, cancellationToken)
            .ConfigureAwait(false);

        UserAccount? user = await _userRepository
            .GetByIdAsync(trimmedUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return Result.Success(MapDto(
                new CitizenProfile(Guid.Empty, trimmedUserId, string.Empty, string.Empty, null, null, CommunicationPreference.None, LanguagePreference.None),
                user));
        }

        return Result.Success(MapDto(profile, user));
    }

    /// <inheritdoc />
    public async Task<Result<CitizenProfileDto>> UpdateProfileAsync(
        UpdateCitizenProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateUpdateRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<CitizenProfileDto>(validation.Error!);
        }

        UpdateCitizenProfile updateProfile = new(
            request.UserId.Trim(),
            request.FirstName.Trim(),
            request.LastName.Trim(),
            new Address(
                request.Address.Street.Trim(),
                request.Address.City.Trim(),
                request.Address.Province.Trim(),
                request.Address.PostalCode.Trim()),
            request.DateOfBirth,
            request.CommunicationPreference,
            request.LanguagePreference);

        Result<CitizenProfile> result = await _citizenProfileRepository
            .UpdateAsync(updateProfile, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return Result.Failure<CitizenProfileDto>(result.Error!);
        }

        UserAccount? user = await _userRepository
            .GetByIdAsync(request.UserId.Trim(), cancellationToken)
            .ConfigureAwait(false);

        LogProfileUpdated(_logger, result.Value!.UserId);
        return Result.Success(MapDto(result.Value, user));
    }

    /// <inheritdoc />
    public async Task<bool> IsProfileCompleteAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        return await _citizenProfileRepository
            .IsProfileCompleteAsync(userId.Trim(), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<CitizenProfileDto>> UpdateIdentityAsync(
        UpdateCitizenIdentityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result validation = ValidateIdentityRequest(request);
        if (validation.IsFailure)
        {
            return Result.Failure<CitizenProfileDto>(validation.Error!);
        }

        string trimmedUserId = request.UserId.Trim();

        UpdateCitizenIdentity identity = new(
            trimmedUserId,
            request.PhoneNumber.Trim(),
            string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim());

        Result<UserAccount> identityResult = await _userRepository
            .UpdateCitizenIdentityAsync(identity, cancellationToken)
            .ConfigureAwait(false);

        if (identityResult.IsFailure)
        {
            return Result.Failure<CitizenProfileDto>(identityResult.Error!);
        }

        CitizenProfile? profile = await _citizenProfileRepository
            .GetByUserIdAsync(trimmedUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return Result.Failure<CitizenProfileDto>(
                new ResultError("citizen_profile.not_found", "The citizen profile could not be found."));
        }

        LogIdentityUpdated(_logger, trimmedUserId);
        return Result.Success(MapDto(profile, identityResult.Value));
    }

    /// <inheritdoc />
    public async Task<Result> SendEmailConfirmationAsync(
        string userId,
        Uri confirmationBaseUrl,
        LanguagePreference languagePreference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmationBaseUrl);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure(
                new ResultError("citizen_profile.invalid_user_id", "A user identifier is required."));
        }

        string trimmedUserId = userId.Trim();

        UserAccount? user = await _userRepository
            .GetByIdAsync(trimmedUserId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(new ResultError("users.not_found", "The requested user could not be found."));
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return Result.Failure(new ResultError("users.email_required", "An email address is required."));
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure(
                new ResultError("users.already_confirmed", "The email address is already confirmed."));
        }

        Result<string> tokenResult = await _userRepository
            .GenerateEmailConfirmationTokenAsync(trimmedUserId, cancellationToken)
            .ConfigureAwait(false);

        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error!);
        }

        string encodedToken = Uri.EscapeDataString(tokenResult.Value!);
        string confirmationLink = $"{confirmationBaseUrl.GetLeftPart(UriPartial.Path)}?userId={Uri.EscapeDataString(trimmedUserId)}&token={encodedToken}";

        EmailConfirmationTokens tokens = new(user.UserName, confirmationLink);
        EmailTemplateResult template = _emailTemplateService.RenderEmailConfirmation(languagePreference, tokens);

        Result sendResult = await _emailSender
            .SendAsync(user.Email, user.UserName, template.Subject, template.HtmlBody, cancellationToken)
            .ConfigureAwait(false);

        if (sendResult.IsFailure)
        {
            return sendResult;
        }

        LogConfirmationEmailSent(_logger, trimmedUserId);
        return Result.Success();
    }

    private static CitizenProfileDto MapDto(CitizenProfile profile, UserAccount? user)
    {
        bool isComplete = !string.IsNullOrWhiteSpace(profile.FirstName)
            && !string.IsNullOrWhiteSpace(profile.LastName)
            && profile.Address is not null
            && !string.IsNullOrWhiteSpace(profile.Address.Street)
            && !string.IsNullOrWhiteSpace(profile.Address.City)
            && !string.IsNullOrWhiteSpace(profile.Address.Province)
            && !string.IsNullOrWhiteSpace(profile.Address.PostalCode)
            && profile.DateOfBirth.HasValue
            && profile.CommunicationPreference != CommunicationPreference.None
            && profile.LanguagePreference != LanguagePreference.None;

        return new CitizenProfileDto(
            profile.Id,
            profile.UserId,
            profile.FirstName,
            profile.LastName,
            profile.Address,
            profile.DateOfBirth,
            profile.CommunicationPreference,
            profile.LanguagePreference,
            isComplete,
            user?.Email,
            user?.PhoneNumber,
            user?.EmailConfirmed ?? false);
    }

    private static Result ValidateIdentityRequest(UpdateCitizenIdentityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result.Failure(
                new ResultError("citizen_profile.invalid_user_id", "A user identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return Result.Failure(
                new ResultError("citizen_profile.phone_number_required", "A phone number is required."));
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !EmailValidator.IsValid(request.Email.Trim()))
        {
            return Result.Failure(
                new ResultError("citizen_profile.email_invalid", "The email address is not valid."));
        }

        return Result.Success();
    }

    private static Result ValidateUpdateRequest(UpdateCitizenProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result.Failure(
                new ResultError("citizen_profile.invalid_user_id", "A user identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return Result.Failure(
                new ResultError("citizen_profile.first_name_required", "A first name is required."));
        }

        if (request.FirstName.Trim().Length > ValidationConstants.FirstNameMaxLength)
        {
            return Result.Failure(
                new ResultError("citizen_profile.first_name_too_long", "The first name must contain at most 256 characters."));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result.Failure(
                new ResultError("citizen_profile.last_name_required", "A last name is required."));
        }

        if (request.LastName.Trim().Length > ValidationConstants.LastNameMaxLength)
        {
            return Result.Failure(
                new ResultError("citizen_profile.last_name_too_long", "The last name must contain at most 256 characters."));
        }

        if (request.Address is null)
        {
            return Result.Failure(
                new ResultError("citizen_profile.address_required", "An address is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Address.Street))
        {
            return Result.Failure(
                new ResultError("citizen_profile.street_required", "A street is required."));
        }

        if (request.Address.Street.Trim().Length > ValidationConstants.StreetMaxLength)
        {
            return Result.Failure(
                new ResultError("citizen_profile.street_too_long",
                    $"The street must contain at most {ValidationConstants.StreetMaxLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(request.Address.City))
        {
            return Result.Failure(
                new ResultError("citizen_profile.city_required", "A city is required."));
        }

        if (request.Address.City.Trim().Length > ValidationConstants.CityMaxLength)
        {
            return Result.Failure(
                new ResultError("citizen_profile.city_too_long",
                    $"The city must contain at most {ValidationConstants.CityMaxLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(request.Address.Province))
        {
            return Result.Failure(
                new ResultError("citizen_profile.province_required", "A province is required."));
        }

        if (request.Address.Province.Trim().Length > ValidationConstants.ProvinceMaxLength)
        {
            return Result.Failure(
                new ResultError("citizen_profile.province_too_long",
                    $"The province must contain at most {ValidationConstants.ProvinceMaxLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(request.Address.PostalCode))
        {
            return Result.Failure(
                new ResultError("citizen_profile.postal_code_required", "A postal code is required."));
        }

        if (request.Address.PostalCode.Trim().Length > ValidationConstants.PostalCodeMaxLength)
        {
            return Result.Failure(
                new ResultError("citizen_profile.postal_code_too_long",
                    $"The postal code must contain at most {ValidationConstants.PostalCodeMaxLength} characters."));
        }

        if (request.DateOfBirth == default)
        {
            return Result.Failure(
                new ResultError("citizen_profile.date_of_birth_required", "A date of birth is required."));
        }

        if (!Enum.IsDefined(request.CommunicationPreference)
            || request.CommunicationPreference == CommunicationPreference.None)
        {
            return Result.Failure(
                new ResultError("citizen_profile.communication_preference_required", "A communication preference is required."));
        }

        if (!Enum.IsDefined(request.LanguagePreference)
            || request.LanguagePreference == LanguagePreference.None)
        {
            return Result.Failure(
                new ResultError("citizen_profile.language_preference_required", "A language preference is required."));
        }

        return Result.Success();
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Updated citizen profile for user {UserId}")]
    private static partial void LogProfileUpdated(ILogger logger, string userId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Updated citizen identity for user {UserId}")]
    private static partial void LogIdentityUpdated(ILogger logger, string userId);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Sent email confirmation for user {UserId}")]
    private static partial void LogConfirmationEmailSent(ILogger logger, string userId);
}
