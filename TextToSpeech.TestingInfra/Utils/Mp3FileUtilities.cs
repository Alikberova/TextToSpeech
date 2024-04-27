namespace TextToSpeech.TestingInfra.Utils;

public class Mp3FileUtilities
{
    private const int MinBufferLength = 10;

    public static bool IsMp3Valid(string audioFilePath)
    {
        using var fileStream = File.OpenRead(audioFilePath);

        var bytes = new byte[MinBufferLength];

        fileStream.Read(bytes, 0, bytes.Length);

        return IsMp3Valid(bytes);
    }

    public static bool IsMp3Valid(byte[] bytes)
    {
        if (bytes.Length < MinBufferLength)
        {
            return false;
        }

        // Check for 'ID3' tag or MPEG audio frame signature
        var isMp3Valid = bytes.Take(3).SequenceEqual(new byte[] { 0x49, 0x44, 0x33 }) ||  // 'ID3'
            (bytes[0] == 0xFF && (bytes[1] & 0xE0) == 0xE0); // MPEG frame sync

        return isMp3Valid;
    }
}
