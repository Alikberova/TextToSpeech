using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Repositories;

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
        if (!Guid.TryParse(fileId, out Guid parsedFileId))
        {
            return BadRequest("Invalid file ID.");
        }

        string filePath = _pathService.ResolveFilePathForStorage(parsedFileId);

        if (!System.IO.File.Exists(filePath))
        {
            var dbAudioFile = await _audioFileRepository.GetAudioFileAsync(Guid.Parse(fileId));

            if (dbAudioFile is not null)
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
}