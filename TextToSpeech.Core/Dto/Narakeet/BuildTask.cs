using System.Text.Json.Serialization;

namespace TextToSpeech.Core.Dto.Narakeet;

public sealed class BuildTask
{
    [JsonPropertyName("statusUrl")]
    public string StatusUrl { get; set; } = string.Empty;

    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;
}