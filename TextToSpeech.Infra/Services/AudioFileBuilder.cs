using static TextToSpeech.Core.Enums;
using TextToSpeech.Core.Entities;
using System.Text;
using System.Security.Cryptography;
using TextToSpeech.Core.Config;

namespace TextToSpeech.Infra.Services;

public static class AudioFileBuilder
{
    public static AudioFile Create(byte[] bytes,
        string voice,
        string langCode,
        double speed,
        AudioType type,
        Guid? ttsApiId = null,
        string? fileName = null,
        Guid? id = null)
    {
        var audio = new AudioFile
        {
            Id = id ?? Guid.NewGuid(),
            FileName = fileName ?? string.Empty,
            Data = bytes,
            CreatedAt = DateTime.UtcNow,
            Description = $"{type}_" +
                $"{voice}_" +
                $"{langCode}_" +
                $"{SharedConstants.TtsApis.FirstOrDefault(kv => kv.Value == ttsApiId ).Key}_" +
                $"{speed}",
            Hash = GenerateAudioFileHash(bytes, voice, langCode, speed),
            Voice = voice,
            LanguageCode = langCode,
            Speed = speed,
            Type = type,
            TtsApiId = ttsApiId
        };
        return audio;
    }

    public static string GenerateAudioFileHash(byte[] bytes, string voice, string languageCode, double speed)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var dataHash = Convert.ToBase64String(SHA256.HashData(bytes));
        var details = $"{dataHash}:{voice}:{languageCode}:{speed}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(details));

        return Convert.ToBase64String(hashBytes);
    }
}
