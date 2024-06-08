using Microsoft.AspNetCore.Http;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services.Common;

public sealed class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public FileStorageService(IPathService pathService)
    {
        _storagePath = pathService.GetFileStoragePath();

        Directory.CreateDirectory(_storagePath);
    }

    public async Task<Guid> StoreFileAsync(IFormFile file)
    {
        string fileContent;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            fileContent = await reader.ReadToEndAsync();
        }

        var fileId = Guid.NewGuid();
        var filePath = Path.Combine(_storagePath, fileId.ToString());

        await File.WriteAllTextAsync(filePath, fileContent);

        return fileId;
    }
}
