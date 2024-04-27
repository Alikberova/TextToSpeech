using Microsoft.Extensions.Logging;

namespace TextToSpeech.Core.Dto;

public sealed record Log
{
    public int Level { get; init; }
    public List<object> Additional { get; init; } = [];
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string FileName { get; init; } = string.Empty;
    public int LineNumber { get; init; }
    public int ColumnNumber { get; init; }
    public LogLevel MappedLogLevel => MapLogLevel(Level);

    private static LogLevel MapLogLevel(int logLevel) =>
        logLevel switch
        {
            1 => LogLevel.Debug,
            2 => LogLevel.Information,
            4 => LogLevel.Warning,
            5 => LogLevel.Error,
            6 => LogLevel.Critical,
            _ => LogLevel.Information,
        };
}
