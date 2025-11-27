using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> Download(string fileId)
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
                return File(dbAudioFile.Data, "audio/mpeg");
            }

            return NotFound("File not found.");
        }

        return File(await System.IO.File.ReadAllBytesAsync(filePath), "audio/mpeg");
    }
}