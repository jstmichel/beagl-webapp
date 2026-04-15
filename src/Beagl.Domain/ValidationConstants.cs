// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain;

/// <summary>
/// Shared validation limits used across application and UI layers.
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Maximum allowed length for a user name.
    /// </summary>
    public const int UserNameMaxLength = 256;

    /// <summary>
    /// Minimum allowed length for a password.
    /// </summary>
    public const int PasswordMinLength = 8;

    /// <summary>
    /// Maximum allowed length for a first name.
    /// </summary>
    public const int FirstNameMaxLength = 256;

    /// <summary>
    /// Maximum allowed length for a last name.
    /// </summary>
    public const int LastNameMaxLength = 256;

    /// <summary>
    /// Maximum allowed length for a street line.
    /// </summary>
    public const int StreetMaxLength = 256;

    /// <summary>
    /// Maximum allowed length for a city name.
    /// </summary>
    public const int CityMaxLength = 100;

    /// <summary>
    /// Maximum allowed length for a province or territory name.
    /// </summary>
    public const int ProvinceMaxLength = 100;

    /// <summary>
    /// Maximum allowed length for a postal code.
    /// </summary>
    public const int PostalCodeMaxLength = 20;

    /// <summary>
    /// Length of the account recovery code.
    /// </summary>
    public const int RecoveryCodeLength = 6;

    /// <summary>
    /// Maximum allowed length for a breed name.
    /// </summary>
    public const int BreedNameMaxLength = 100;

    /// <summary>
    /// Maximum allowed length for a color name.
    /// </summary>
    public const int ColorNameMaxLength = 100;
}
