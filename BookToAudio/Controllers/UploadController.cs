using BookToAudio.Infra.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookToAudio.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
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
