using BookToAudio.Infra.Dto;
using BookToAudio.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookToAudio.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpeechController : ControllerBase
{
    private readonly SpeechService _speechService;

    public SpeechController(SpeechService speechService)
    {
        _speechService = speechService;
    }

    // POST api/<SpeechController>
    [HttpPost]
    public IActionResult CreateSpeech([FromForm] SpeechRequest request)
    {
        var fileId = _speechService.CreateSpeech(request);

        return Ok(fileId);
    }

    [HttpPost("sample")]
    public async Task<IActionResult> GetSpeechSample([FromBody] SpeechRequest request)
    {
        var audioStream = await _speechService.CreateSpeechSample(request);

        return new FileStreamResult(audioStream, "audio/mpeg");
    }
}
