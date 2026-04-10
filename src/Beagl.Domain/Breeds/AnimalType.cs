// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Domain.Breeds;

/// <summary>
/// Represents the type of animal a breed belongs to.
/// </summary>
public enum AnimalType
{
    /// <summary>
    /// No animal type specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// A cat breed.
    /// </summary>
    Cat = 1,

    /// <summary>
    /// A dog breed.
    /// </summary>
    Dog = 2,
}
