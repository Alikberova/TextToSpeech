using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UploadController : ControllerBase //todo del
{
    private readonly IFileStorageService _fileStorageService;

    public UploadController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var fileId = await _fileStorageService.StoreFileAsync(file);

        return Ok(new { FileId = fileId });
    }
}
