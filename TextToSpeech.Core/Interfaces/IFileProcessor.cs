using Microsoft.AspNetCore.Http;

namespace TextToSpeech.Core.Interfaces;

public interface IFileProcessor
{
    bool CanProcess(string fileType);
    Task<string> ExtractContentAsync(IFormFile file);
}