// MIT License - Copyright (c) 2025 Jonathan St-Michel

using Microsoft.Extensions.Logging;

namespace Beagl.Application.Tests.Utilities;

public class FakeLogger<T> : ILogger<T>
{
    private readonly List<LogEntry> _logs = new();

    public IReadOnlyList<LogEntry> Logs => _logs.AsReadOnly();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        var message = formatter(state, exception);
        _logs.Add(new LogEntry(logLevel, eventId, message, exception));
    }

    public record LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception);
}
