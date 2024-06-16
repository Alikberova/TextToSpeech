using static TextToSpeech.Core.Enums;
using TextToSpeech.Core.Entities;
using System.Text;
using System.Security.Cryptography;

namespace TextToSpeech.Infra.Services;

internal class AudioFileBuilder
{
    public static AudioFile Create(byte[] bytes,
        string descr,
        string voice,
        string langCode,
        double speed,
        AudioType type,
        string? fileName = null,
        Guid? id = null,
        Guid? ttsApiId = null)
    {
        var audio = new AudioFile
        {
            Id = id ?? Guid.NewGuid(),
            FileName = fileName ?? string.Empty,
            Data = bytes,
            CreatedAt = DateTime.UtcNow,
            Description = descr,
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
        var dataHash = Convert.ToBase64String(SHA256.HashData(bytes));
        var details = $"{dataHash}:{voice}:{languageCode}:{speed}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(details));

        return Convert.ToBase64String(hashBytes);
    }
}
