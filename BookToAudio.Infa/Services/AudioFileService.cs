namespace BookToAudio.Infra.Services;

public interface IAudioFileService
{
    byte[] ConcatenateMp3Files(ReadOnlyMemory<byte>[] audioFiles);
}

public class AudioFileService : IAudioFileService
{
    public byte[] ConcatenateMp3Files(ReadOnlyMemory<byte>[] audioFiles)
    {
        using var memoryStream = new MemoryStream();

        foreach (var file in audioFiles)
        {
            // Convert ReadOnlyMemory<byte> to byte[] and write to the MemoryStream
            byte[] buffer = file.ToArray();
            memoryStream.Write(buffer, 0, buffer.Length);
        }

        return memoryStream.ToArray();
    }
}
