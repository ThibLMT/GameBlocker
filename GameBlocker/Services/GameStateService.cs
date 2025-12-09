using System.Collections.Concurrent; // Thread-safe collections

namespace GameBlocker.Services;

public class GameStateService
{
    // 1. Service Control
    public bool IsEnabled { get; set; } = true;

    // 2. Stats
    // Interlocked.Increment is safer for simple ints, but volatile is okay for just reading
    private int _killCount = 0;
    public int KillCount => _killCount;

    // 3. Logs
    // ConcurrentQueue is thread-safe. We don't want the API crashing 
    // because the Worker is writing a log at the exact same millisecond.
    private readonly ConcurrentQueue<LogEntry> _logs = new();

    public IEnumerable<LogEntry> GetRecentLogs() => _logs.Take(50); // Return last 50

    public void IncrementKillCount()
    {
        Interlocked.Increment(ref _killCount);
    }

    public void AddLog(string message)
    {
        var entry = new LogEntry
        {
            Id = DateTime.Now.Ticks, // Simple ID
            Message = message,
            Timestamp = DateTime.Now.ToLongTimeString()
        };

        _logs.Enqueue(entry);

        // Keep size manageable
        if (_logs.Count > 100)
            _logs.TryDequeue(out _);
    }
}

// Simple DTO for the log
public class LogEntry
{
    public long Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}