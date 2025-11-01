﻿using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Core.Entities;

public sealed class AudioFile
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public string Description { get; init; } = string.Empty;
    public Status Status { get; set; }
    public string Hash { get; init; } = string.Empty;
    public string Voice { get; init; } = string.Empty;
    public string LanguageCode { get; init; } = string.Empty;
    public double Speed { get; init; }
    public AudioType Type { get; init; }

    public Guid? TtsApiId { get; set; }
    public TtsApi? TtsApi { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is AudioFile other
            && Hash == other.Hash
            && Data.SequenceEqual(other.Data);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Hash);
        hashCode.AddBytes(Data);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(AudioFile left, AudioFile right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(AudioFile left, AudioFile right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{Id}: {Description}";
        // return $"{Id}_{Type}_{Voice}_{LanguageCode}_{Speed}_{TtsApi?.Name}";
    }
}
