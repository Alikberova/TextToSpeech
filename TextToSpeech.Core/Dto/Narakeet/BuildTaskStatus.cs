using System.Text.Json.Serialization;

namespace TextToSpeech.Core.Dto.Narakeet;

public sealed class BuildTaskStatus
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("percent")]
    public int Percent { get; set; }

    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }

    [JsonPropertyName("finished")]
    public bool Finished { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;
}