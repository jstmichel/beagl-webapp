// MIT License - Copyright (c) 2025 Jonathan St-Michel

using System.Diagnostics.CodeAnalysis;

namespace Beagl.Infrastructure.Database;

/// <summary>
/// EF Core owned type representing a postal address. Reusable across entities.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddressEntity
{
    /// <summary>
    /// Gets or sets the street line.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city or municipality.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the province or territory.
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
}
