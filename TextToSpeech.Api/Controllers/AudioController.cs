using TextToSpeech.Infra.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace TextToSpeech.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AudioController : ControllerBase
{
    private readonly IPathService _pathService;

    public AudioController(IPathService pathService)
    {
        _pathService = pathService;
    }

    [HttpGet("downloadmp3/{fileId}/{fileName}")]
    public IActionResult DownloadMp3(string fileId, string fileName)
    {
        string filePath = _pathService.GetFileStorageFilePath($"{fileId}.mp3");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        var contentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName
        };

        Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();

        return File(System.IO.File.ReadAllBytes(filePath), "audio/mpeg");
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
