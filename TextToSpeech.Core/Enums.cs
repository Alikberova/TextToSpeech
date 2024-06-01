namespace TextToSpeech.Core;

public sealed class Enums
{
    public enum Status
    {
        Created,
        Processing,
        Completed,
        Failed,
        Cancelled
    }

    public enum AudioType
    {
        Unspecified,
        Sample,
        Full
    }
}
