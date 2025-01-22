﻿namespace TextToSpeech.Core.Dto.Narakeet;

public sealed record VoiceResponse
{
    public string Name { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string LanguageCode { get; init; } = string.Empty;
    public List<string> Styles { get; init; } = [];
}
