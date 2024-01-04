using BookToAudio.Infra.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookToAudio.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AudioController : ControllerBase
{
    private readonly IPathService _pathService;

    public AudioController(IPathService pathService)
    {
        _pathService = pathService;
    }

    [HttpGet]
    [Route("downloadmp3/{fileId}")]
    public IActionResult DownloadMp3(string fileId)
    {
        // Replace this with your logic to get the file path based on the fileId
        string filePath = _pathService.GetFileStorageFilePath($"{fileId}.mp3");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "audio/mpeg", Path.GetFileName(filePath));
    }

    [HttpGet]
    [Route("streammp3/{fileId}")]
    public async Task<IActionResult> StreamMp3(string fileId)
    {
        string filePath = _pathService.GetFileStorageFilePath($"{fileId}.mp3");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        var memoryStream = new MemoryStream();
        using (var stream = new FileStream(filePath, FileMode.Open))
        {
            await stream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;
        return File(memoryStream, "audio/mpeg", Path.GetFileName(filePath), true);
    }
}
