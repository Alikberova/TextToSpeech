namespace BookToAudio.Infra.Services;

public interface IFileStorageService
{
    Task<string> StoreFileAsync(string fileContent);
    Task<string> RetrieveFileAsync(string fileId);
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

    public async Task<string> RetrieveFileAsync(string fileId)
    {
        string filePath = Path.Combine(_storagePath, fileId);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The file was not found.", fileId);
        }

        return await File.ReadAllTextAsync(filePath);
    }
}
