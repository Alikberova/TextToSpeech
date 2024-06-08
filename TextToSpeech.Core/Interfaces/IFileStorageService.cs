using Microsoft.AspNetCore.Http;

namespace TextToSpeech.Core.Interfaces;

public interface IFileStorageService
{
    Task<Guid> StoreFileAsync(IFormFile file);
}