using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Captures log level + formatted message for every <see cref="Log"/> call, so tests can
/// assert on log output (e.g. the "Shell Navigation Canceled." line) without relying on
/// event-args state.
/// </summary>
public class RecordingLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }

    public bool HasEntry(LogLevel logLevel, string message)
    {
        return Entries.Any(x => x.Level == logLevel && x.Message == message);
    }

    public bool HasEntryContaining(LogLevel logLevel, string fragment)
    {
        return Entries.Any(x => x.Level == logLevel && x.Message.Contains(fragment, StringComparison.Ordinal));
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}

/// <summary>
/// A single captured log line.
/// </summary>
/// <param name="Level">The <see cref="LogLevel"/>.</param>
/// <param name="Message">The formatted message.</param>
/// <param name="Exception">The related <see cref="Exception"/> (if any).</param>
public sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
