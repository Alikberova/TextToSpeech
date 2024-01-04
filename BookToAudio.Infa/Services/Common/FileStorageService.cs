namespace BookToAudio.Infra.Services.Common;

public interface IFileStorageService
{
    Task<string> StoreFileAsync(string fileContent);
}

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public FileStorageService(IPathService pathService)
    {
        _storagePath = pathService.GetFileStoragePath();

        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> StoreFileAsync(string fileContent)
    {
        string fileId = Guid.NewGuid().ToString();
        string filePath = Path.Combine(_storagePath, fileId);

        await File.WriteAllTextAsync(filePath, fileContent);

        return fileId;
    }
}
