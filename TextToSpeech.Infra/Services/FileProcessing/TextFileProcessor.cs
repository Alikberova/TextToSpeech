using TextToSpeech.Infra.Services.Common;
using TextToSpeech.Infra.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TextToSpeech.Infra.Services.FileProcessing;

public sealed class TextFileProcessor : IFileProcessor
{
    private readonly IPathService _pathService;

    public TextFileProcessor(IPathService pathService)
    {
        _pathService = pathService;
    }

    public bool CanProcess(string fileType) => fileType.Equals(".txt", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractContentAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return await Task.FromResult(string.Empty);
        }

        using var reader = new StreamReader(file.OpenReadStream());
        return await reader.ReadToEndAsync();
    }
}
