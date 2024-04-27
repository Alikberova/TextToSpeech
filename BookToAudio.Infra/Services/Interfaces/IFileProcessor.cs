using Microsoft.AspNetCore.Http;

namespace BookToAudio.Infra.Services.Interfaces;

public interface IFileProcessor
{
    bool CanProcess(string fileType);
    Task<string> ExtractContentAsync(IFormFile file);
}