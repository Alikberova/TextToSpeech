using static TextToSpeech.Core.Enums;
using TextToSpeech.Core.Entities;
using System.Text;
using System.Security.Cryptography;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Dto;

namespace TextToSpeech.Infra.Services;

public static class AudioFileBuilder
{
    public static AudioFile Create(byte[] bytes,
        string langCode,
        AudioType type,
        string input,
        TtsRequestOptions options,
        Guid? ttsApiId = null,
        string? fileName = null,
        Guid? id = null,
        string? hash = null)
    {
        var audio = new AudioFile
        {
            Id = id ?? Guid.NewGuid(),
            FileName = fileName ?? string.Empty,
            Data = bytes,
            CreatedAt = DateTime.UtcNow,
            Description = $"{type}_" +
                $"{options.Voice}_" +
                $"{langCode}_" +
                $"{SharedConstants.TtsApis.FirstOrDefault(kv => kv.Value == ttsApiId ).Key}_" +
                $"{options.Speed}_" +
                $"{options.Model}",
            Hash = hash ?? GenerateHash(input, langCode, options),
            Voice = options.Voice,
            LanguageCode = langCode,
            Speed = options.Speed,
            Type = type,
            TtsApiId = ttsApiId
        };
        return audio;
    }

    public static string GenerateHash(string input, string languageCode, TtsRequestOptions options)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        var dataHash = Convert.ToBase64String(SHA256.HashData(bytes));
        var details = $"{dataHash}:{options.Voice}:{languageCode}:{options.Speed}:{options.Model}:" +
            $"{options.ResponseFormat}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(details));

        return Convert.ToBase64String(hashBytes);
    }
}
