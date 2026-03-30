// MIT License - Copyright (c) 2025 Jonathan St-Michel

namespace Beagl.Application.Setup.Services;

/// <summary>
/// Thread-safe, in-memory cache for the setup-completed status.
/// Register as a singleton so the flag persists across scoped service lifetimes.
/// </summary>
public sealed class SetupStatusCache
{
    private volatile bool _isSetupComplete;

    /// <summary>
    /// Gets a value indicating whether initial setup has been completed.
    /// </summary>
    public bool IsSetupComplete => _isSetupComplete;

    /// <summary>
    /// Marks the initial setup as completed.
    /// </summary>
    public void MarkSetupComplete()
    {
        _isSetupComplete = true;
    }
}
