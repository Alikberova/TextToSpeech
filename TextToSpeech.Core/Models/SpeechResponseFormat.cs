﻿using System.ComponentModel;
using System.Text.Json.Serialization;
using TextToSpeech.Core.Converters;

namespace TextToSpeech.Core.Models;

[TypeConverter(typeof(SpeechResponseTypeConverter))]
[JsonConverter(typeof(SpeechResponseJsonConverter))]
public struct SpeechResponseFormat
{
    private readonly string _value;
    private static readonly HashSet<string> _valid = [Mp3Value, OpusValue, AacValue, FlacValue,
        WavValue, PcmValue];

    private const string Mp3Value = "mp3";
    private const string OpusValue = "opus";
    private const string AacValue = "aac";
    private const string FlacValue = "flac";
    private const string WavValue = "wav";
    private const string PcmValue = "pcm";

    public SpeechResponseFormat(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!_valid.Contains(value))
        {
            throw new ArgumentException(UnsupportedFormatError(value), nameof(value));
        }

        _value = value;
    }

    public static SpeechResponseFormat Mp3 { get; } = new SpeechResponseFormat(Mp3Value);

    public static SpeechResponseFormat Opus { get; } = new SpeechResponseFormat(OpusValue);

    public static SpeechResponseFormat Aac { get; } = new SpeechResponseFormat(AacValue);

    public static SpeechResponseFormat Flac { get; } = new SpeechResponseFormat(FlacValue);

    public static SpeechResponseFormat Wav { get; } = new SpeechResponseFormat(WavValue);

    public static SpeechResponseFormat Pcm { get; } = new SpeechResponseFormat(PcmValue);

    public override readonly string ToString()
    {
        return _value;
    }

    public static bool TryParse(string? str, out SpeechResponseFormat value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(str) || !_valid.Contains(str))
        {
            return false;
        }

        value = new SpeechResponseFormat(str);

        return true;
    }

    public static string UnsupportedFormatError(string? value) =>
        $"Unsupported response format: '{value}'. Allowed: {string.Join(", ", _valid)}.";
}