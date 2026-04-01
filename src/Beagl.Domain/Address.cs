// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain;

/// <summary>
/// Represents a postal address. Reusable across domain entities.
/// </summary>
/// <param name="Street">The street line (e.g. civic number and street name).</param>
/// <param name="City">The city or municipality.</param>
/// <param name="Province">The province or territory.</param>
/// <param name="PostalCode">The postal code.</param>
public sealed record Address(
    string Street,
    string City,
    string Province,
    string PostalCode);
