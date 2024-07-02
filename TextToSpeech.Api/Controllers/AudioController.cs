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
        string filePath = _pathService.GetFilePathInFileStorage($"{fileId}.mp3");

        if (!System.IO.File.Exists(filePath))
        {
            var dbAudioFile = await _audioFileRepository.GetAudioFileAsync(Guid.Parse(fileId));

            Console.WriteLine("file Description" + dbAudioFile.Description);
            Console.WriteLine("file FileName" + dbAudioFile.FileName);
            Console.WriteLine("file Hash" + dbAudioFile.Hash);
            Console.WriteLine("file count" + dbAudioFile.Data.Count());
            Console.WriteLine("file CreatedAt" + dbAudioFile.CreatedAt);
            Console.WriteLine("file LanguageCode" + dbAudioFile.LanguageCode);
            Console.WriteLine("file Speed" + dbAudioFile.Speed);

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
}