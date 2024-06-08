using TextToSpeech.Infra.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TextToSpeech.Core.Repositories;

namespace TextToSpeech.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AudioController : ControllerBase
{
    private readonly IPathService _pathService;
    private readonly IAudioFileRepository _audioFileRepository;

    public AudioController(IPathService pathService, IAudioFileRepository audioFileRepository)
    {
        _pathService = pathService;
        _audioFileRepository = audioFileRepository;
    }

    [HttpGet("downloadmp3/{fileId}/{fileName}")]
    public async Task<IActionResult> DownloadMp3(string fileId, string fileName)
    {
        string filePath = _pathService.GetFileStorageFilePath($"{fileId}.mp3");

        if (!System.IO.File.Exists(filePath))
        {
            var dbAudioFile = await _audioFileRepository.GetAudioFileAsync(Guid.Parse(fileId));

            if (dbAudioFile != null)
            {
                SetHeader(fileName);
                return File(dbAudioFile.Data, "audio/mpeg");
            }

            return NotFound("File not found.");
        }

        SetHeader(fileName);

        return File(System.IO.File.ReadAllBytes(filePath), "audio/mpeg");
    }

    private void SetHeader(string fileName)
    {
        var contentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName
        };

        Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
    }

    [HttpGet]
    [Route("streammp3/{fileId}")]
    public async Task<IActionResult> StreamMp3(string fileId) //todo remove
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
